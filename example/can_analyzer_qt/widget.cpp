#include "widget.h"
#include "ui_widget.h"

#include <QMessageBox>
#include <QDateTime>
#include <QtNetwork/QNetworkInterface>

bool parseIpAddress(const QString &ipString, uint8_t *ipArray)
{
       QStringList parts = ipString.split('.');
       if (parts.size() != 4) {
           return false;
       }

       for (int i = 0; i < 4; ++i) {
           bool ok;
           int value = parts[i].toInt(&ok);
           if (!ok || value < 0 || value > 255) {
               return false;
           }
           ipArray[i] = static_cast<uint8_t>(value);
       }
       return true;
}

Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
    , msgTableModel(NULL)
    , pendingTxMsgCount(0)
    , openedChannelCount(0)
{
    memset(channels, NULL, sizeof(channels));
    ui->setupUi(this);
    ui->modeCombo->clear();
    ui->modeCombo->addItem(tr("Normal"), (int)BM_CAN_NORMAL_MODE);
    ui->modeCombo->addItem(tr("CAN Only"), (int)BM_CAN_CLASSIC_MODE);
    ui->modeCombo->addItem(tr("Loopback"), (int)BM_CAN_EXTERNAL_LOOPBACK_MODE);
    ui->modeCombo->addItem(tr("Listen only"), (int)BM_CAN_LISTEN_ONLY_MODE);
    getAvailableHostIpList();
    msgTableModel = new CanMsgTableModel;
    ui->msgTableView->setModel(msgTableModel);
    ui->msgTableView->horizontalHeader()->setSectionResizeMode(ui->msgTableView->horizontalHeader()->count() - 1, QHeaderView::Stretch);

    ui->msgTableView->horizontalHeader()->setSectionResizeMode(0, QHeaderView::Fixed);
    ui->msgTableView->horizontalHeader()->resizeSection(0, 200);
    rxTimer = new QTimer(this);
    connect(rxTimer, &QTimer::timeout, this, &Widget::readPendingMessages);

    txTimer = new QTimer(this);
    connect(txTimer, &QTimer::timeout, this, &Widget::writePendingMessages);

    recoverFromErrorTimer = new QTimer(this);
    connect(recoverFromErrorTimer, &QTimer::timeout, [=]() { this->recoverFromError(-1/* check all channels */); });

    BM_Init();
    enumerate();
}

Widget::~Widget()
{
    BM_UnInit();
    delete ui;
}

int Widget::lengthToDlc(int n)
{
    int dlc = 0;
    if (n <= 8) {
        dlc = n;
    }
    else if (n <= 12) {
        dlc = 9;
    }
    else if (n <= 16) {
        dlc = 10;
    }
    else if (n <= 20) {
        dlc = 11;
    }
    else if (n <= 24) {
        dlc = 12;
    }
    else if (n <= 32) {
        dlc = 13;
    }
    else if (n <= 48) {
        dlc = 14;
    }
    else if (n <= 64) {
        dlc = 15;
    }
    else
    {
        dlc = 15;
    }
    return dlc;
}

void Widget::enumerate()
{
    bool succeed = false;
    uint8_t ip[4] = { 192, 168, 41, 255 };
    bool remote = parseIpAddress(ui->hostComboBox->currentText(), ip);
    int n = sizeof(channelinfos) / sizeof(channelinfos[0]);
    ui->channelCombo->clear();
    if (remote)
    {
        BM_ChannelInfoTypeDef* remotechannelinfos = new BM_ChannelInfoTypeDef[n];
        if (BM_ERROR_OK == BM_EnumerateRemote(remotechannelinfos, &n, ip, 100))
        {
            int ncan = 0;
            for (int i = 0; i < n; i++)
            {
                if (remotechannelinfos[i].cap & BM_CAN_CAP)
                {
                    channelinfos[ncan++] = remotechannelinfos[i];
                }
            }
            n = ncan;
            succeed = true;
        }
        delete[] remotechannelinfos;
    }
    else
    {
        if (BM_ERROR_OK == BM_EnumerateByCap(channelinfos, &n, BM_CAN_CAP))
        {
            succeed = true;
        }
    }
    if (succeed)
    {
        for (int i = 0; i < n; i++)
        {
            ui->channelCombo->appendItem(channelinfos[i].name, true);
        }
    }
}

void Widget::getAvailableHostIpList() const
{
    QStringList allips;
    QList<QNetworkInterface> interfaceList = QNetworkInterface::allInterfaces();
    foreach(QNetworkInterface interfaceItem, interfaceList)
    {
        if (interfaceItem.flags().testFlag(QNetworkInterface::CanBroadcast)
                && interfaceItem.flags().testFlag(QNetworkInterface::CanMulticast)
                && !interfaceItem.flags().testFlag(QNetworkInterface::IsLoopBack)
                && !interfaceItem.hardwareAddress().startsWith("00:50:56")
                && !interfaceItem.humanReadableName().toLower().contains("vmware"))
        {
            QString mac = interfaceItem.hardwareAddress();
            QList<QNetworkAddressEntry> ips = interfaceItem.addressEntries();
            Q_FOREACH(auto entry, ips)
            {
                QString ip = entry.ip().toString();
                // 排除 IPv6 链路本地地址
                if (!ip.startsWith("fe80::"))
                {
                    allips.append(ip);
                }
            }
        }
    }
    allips.append("127.0.0.1");
    allips.removeDuplicates();
    for (int i = 0; i < allips.size(); ++i)
    {
        if (allips[i] != "127.0.0.1")
        {
            int lastDotIndex = allips[i].lastIndexOf('.');
            if (lastDotIndex != -1)
            {
                QString prefix = allips[i].left(lastDotIndex + 1);
                allips[i] = prefix + "255";
            }
        }
        ui->hostComboBox->addItem(allips[i]);
    }
}

void Widget::on_openButton_clicked(bool checked)
{
    bool success = true;
    ui->sendcomboBox->clear();
    if (checked)
    {
        openedChannelCount = 0;
        for (int i = 0; i < ui->channelCombo->count(); i++)
        {
            if (ui->channelCombo->isChecked(i) == true)
            {
                BM_ChannelInfoTypeDef* channelinfo = &channelinfos[i]; // Iterate all enumerated ports here
                ui->sendcomboBox->addItem(QString::number(openedChannelCount + 1)); // Show opened port index here

                BM_CanModeTypeDef mode = (BM_CanModeTypeDef)ui->modeCombo->currentData().value<int>();
                BM_TerminalResistorTypeDef tres = ui->tresCheck->isChecked() ? BM_TRESISTOR_120 : BM_TRESISTOR_DISABLED;
                BM_BitrateTypeDef bitrate;
                memset(&bitrate, 0, sizeof(bitrate));
                bitrate.nbitrate = ui->nominalBitrateEdit->text().toUInt();
                bitrate.dbitrate = ui->dataBitrateEdit->text().toUInt();
                bitrate.nsamplepos = ui->samplePosSpin->value();
                bitrate.dsamplepos = bitrate.nsamplepos;
                BM_RxFilterTypeDef rxfilter;
                memset(&rxfilter, 0, sizeof(rxfilter));
                rxfilter.type = ui->rxAdvancedFilterCheck->isChecked() ? BM_RXFILTER_ADVANCED : BM_RXFILTER_BASIC;
                uint rxId = ui->rxIdEdit->text().toUInt(NULL, 16);
                uint rxMask = ui->rxMaskEdit->text().toUInt(NULL, 16);
                bool rxStd = ui->rxStandardCheck->isChecked();
                bool rxExt = ui->rxExtendedCheck->isChecked();
                if (rxId > 0x7FF || rxMask > 0x7FF)
                {
                    rxfilter.id_mask = ((rxMask >> 18) & 0x7FF) | ((rxMask & 0x3FFFF) << 11);
                    rxfilter.id_value = ((rxId >> 18) & 0x7FF) | ((rxId & 0x3FFFF) << 11);
                }
                else
                {
                    rxfilter.id_mask = rxMask;
                    rxfilter.id_value = rxId;
                }
                if (rxStd && !rxExt)
                {
                    rxfilter.flags_mask = BM_MESSAGE_FLAGS_IDE;
                    rxfilter.flags_value = 0;
                }
                else if (!rxStd && rxExt)
                {
                    rxfilter.flags_mask = BM_MESSAGE_FLAGS_IDE;
                    rxfilter.flags_value = BM_MESSAGE_FLAGS_IDE;
                }
                else
                {
                    /* Default as all enabled */
                    ui->rxStandardCheck->setChecked(true);
                    ui->rxExtendedCheck->setChecked(true);
                }
                if (rxfilter.type == BM_RXFILTER_ADVANCED)
                {
                    memset(rxfilter.payload_mask, 0, sizeof(rxfilter.payload_mask));
                    memset(rxfilter.payload_value, 0, sizeof(rxfilter.payload_value));
                    int j = 0;
                    QString payload = ui->rxDataMaskEdit->text().replace(QLatin1Char('x'), QLatin1Char('0'), Qt::CaseInsensitive);
                    while (j*2 + 2 <= payload.length())
                    {
                        rxfilter.payload_mask[j] = payload.mid(j*2, 2).toUInt(NULL, 16);
                        rxfilter.payload_value[j] = rxfilter.payload_mask[j];
                        j++;
                    }
                }
                BM_StatusTypeDef status = BM_OpenEx(&channels[openedChannelCount], channelinfo, mode, tres, &bitrate, &rxfilter, 1);
                if (status != BM_ERROR_OK)
                {
                    char buffer[1024];
                    BM_GetErrorText(status, buffer, sizeof(buffer), 0);
                    QString prompt = QString(tr("Failed to open channel %1, error=%2(%3)."))
                            .arg(channelinfo->name)
                            .arg((int)status, 8, 16)
                            .arg(buffer);
                    QMessageBox::critical(this, tr("Error"), prompt);
                    success = false;
                }
                BM_SetPtpMode(channels[openedChannelCount], BM_PTP_INPUT_USB_SOF);
                memcpy(&channelRecoveryConfigs[openedChannelCount].info, channelinfo, sizeof(*channelinfo));
                memcpy(&channelRecoveryConfigs[openedChannelCount].mode, &mode, sizeof(mode));
                memcpy(&channelRecoveryConfigs[openedChannelCount].tres, &tres, sizeof(tres));
                memcpy(&channelRecoveryConfigs[openedChannelCount].bitrate, &bitrate, sizeof(bitrate));
                memcpy(&channelRecoveryConfigs[openedChannelCount].rxfilters[0], &rxfilter, sizeof(rxfilter));
                openedChannelCount++;
            }
        }

        // Sync PTP timestamp for all devices
        if (BM_SyncPtpTimes(channels, openedChannelCount) != BM_ERROR_OK)
        {
            qWarning("Failed to sync PTP timestamps.\n");
        }
    }

    if (checked && success)
    {
        msgTableModel->clearAllMessages();
        rxTimer->start(50);
        recoverFromErrorTimer->start(1000);
        ui->openButton->setText(tr("Close"));
        ui->rxIdEdit->setEnabled(false);
        ui->rxMaskEdit->setEnabled(false);
        ui->rxExtendedCheck->setEnabled(false);
        ui->rxStandardCheck->setEnabled(false);
        //ui->rxAdvancedFilterCheck->setEnabled(false);
        ui->rxDataMaskEdit->setEnabled(false);
        ui->modeCombo->setEnabled(false);
        ui->tresCheck->setEnabled(false);
        ui->nominalBitrateEdit->setEnabled(false);
        ui->dataBitrateEdit->setEnabled(false);
        ui->samplePosSpin->setEnabled(false);
        ui->enumerateButton->setEnabled(false);
        ui->txButton->setEnabled(true);
        ui->channelCombo->setEnabled(false);
    }
    else
    {
        rxTimer->stop();
        recoverFromErrorTimer->stop();
        ui->openButton->setChecked(false);
        ui->openButton->setText(tr("Open"));
        ui->rxIdEdit->setEnabled(true);
        ui->rxMaskEdit->setEnabled(true);
        ui->rxExtendedCheck->setEnabled(true);
        ui->rxStandardCheck->setEnabled(true);
        //ui->rxAdvancedFilterCheck->setEnabled(true);
        ui->rxDataMaskEdit->setEnabled(ui->rxAdvancedFilterCheck->isChecked());
        ui->modeCombo->setEnabled(true);
        ui->tresCheck->setEnabled(true);
        ui->nominalBitrateEdit->setEnabled(true);
        ui->dataBitrateEdit->setEnabled(true);
        ui->samplePosSpin->setEnabled(true);
        ui->enumerateButton->setEnabled(true);
        ui->txButton->setEnabled(false);
        ui->channelCombo->setEnabled(true);
        for (int i = 0; i < openedChannelCount; i++)
        {
            BM_Close(channels[i]);
            channels[i] = nullptr;
        }
        openedChannelCount = 0;
    }

    if (success == false)
    {
        QMessageBox::critical(this, tr("Error"), tr("Please plugin your Busmust device and press 'Enumerate'."));
    }
}

void Widget::on_enumerateButton_clicked()
{
    enumerate();
}

void Widget::on_txButton_clicked(bool checked)
{
    bool success = false;
    int port = ui->sendcomboBox->currentIndex();
    if (port >= 0 && checked && channels[port] && port < int(sizeof(channels) / sizeof(channels[0])))
    {
        BM_CanMessageTypeDef* canMsg = (BM_CanMessageTypeDef*)pendingTxMsg.payload;
        uint id = ui->txIdEdit->text().toUInt(NULL, 16);
        memset(&pendingTxMsg, 0, sizeof(pendingTxMsg));
        canMsg->ctrl.tx.BRS = ui->txFdCheck->isChecked();
        canMsg->ctrl.tx.FDF = ui->txFdCheck->isChecked();
        canMsg->ctrl.tx.IDE = ui->txExtendedCheck->isChecked();
        if (canMsg->ctrl.tx.IDE)
        {
            canMsg->id.SID = (id >> 18) & 0x7FF;
            canMsg->id.EID = (id & 0x3FFFF);
        }
        else
        {
            canMsg->id.SID = id & 0x7FF;
        }
        uint length = ui->txLengthEdit->text().toUInt();
        int dlc = lengthToDlc(length);
        uint dlcToDataBytes[16] = {0,1,2,3,4,5,6,7,8,12,16,20,24,32,48,64};
        int dlcToLength = dlcToDataBytes[dlc];
        if (dlcToLength != length)
        {
            QMessageBox::warning(this, tr("Warning"), tr("According to ISO standard, CANFD message payload length could only be 0-8,12,16,20,24,32,48 or 64."));
        }
        canMsg->ctrl.tx.DLC = dlc;
        QString payload = ui->txDataEdit->text().remove(QLatin1Char(' '));
        int j = 0;
        while (j*2 + 2 <= payload.length())
        {
            canMsg->payload[j] = payload.mid(j*2, 2).toUInt(NULL, 16);
            j++;
        }
        pendingTxMsg.header.type = BM_CAN_FD_DATA;
        pendingTxMsg.length = sizeof(BM_CanMessageTypeDef);
        success = true;
    }


    if (checked && success)
    {
        pendingTxMsgCount = ui->txCountEdit->text().toUInt();
        txTimer->start(ui->txCycleEdit->text().toUInt());
        ui->txButton->setText(tr("Stop"));
        writePendingMessages();
    }
    else
    {
        txTimer->stop();
        pendingTxMsgCount = 0;
        ui->txButton->setChecked(false);
        ui->txButton->setText(tr("Transmit"));
    }
}

void Widget::readPendingMessages(void)
{
    BM_DataTypeDef msg;
    bool newMsgInserted = false;
    for (int i = 0; i < openedChannelCount; i++)
    {
        if (channels[i] && msgTableModel)
        {
            while (BM_Read(channels[i], &msg) == BM_ERROR_OK)
            {
                MsgDataTypeDef data;
                data.msg = msg;
                data.port = i + 1;
                if (BM_GetDataPtpTimestamp(channels[i], &msg, &data.utcts) != BM_ERROR_OK)
                {
                    // If failed to extract a PTP timestamp from data, use current time on host-machine.
                    data.utcts = BM_GetHostPtpTime();
                }
                msgTableModel->insertMessage(data);
                newMsgInserted = true;
            }
        }
        if (newMsgInserted)
        {
            ui->msgTableView->scrollToBottom();
        }
    }
}

void Widget::writePendingMessages(void)
{
    int port = ui->sendcomboBox->currentIndex();
    if (channels[port] && msgTableModel)
    {
        if (pendingTxMsgCount > 0)
        {
            uint32_t timestamp = 0;
            BM_StatusTypeDef status = BM_Write(channels[port], &pendingTxMsg, 0, &timestamp);
            if (status == BM_ERROR_OK)
            {
                if (--pendingTxMsgCount <= 0)
                {
                    on_txButton_clicked(false);
                }
            }
            else
            {
                recoverFromError(port);
            }
        }
    }
}


void Widget::recoverFromError(int port /* = -1*/)
{
#ifdef QT_VERSION_MAJOR
#define DEMO_PRINT qWarning
#elif defined(_WIN32)
#define DEMO_PRINT OutputDebugString
#else
#define DEMO_PRINT printf
#endif

    // Note: It takes some time to run BM_Close and BM_OpenEx,
    // and channel handles will be invalidated when recovering from error,
    // make sure no other threads are using channel handles (e.g. calling BM_Write) during the recovery,
    // or, use single-threaded design, just like this demo.
    for (int i = 0; i < openedChannelCount; i++)
    {
        uint64_t currentTs = QDateTime::currentMSecsSinceEpoch();
        uint64_t elapsed = currentTs - channelRecoveryConfigs[i].previousRecoveryTs;
        if (elapsed >= 1000ULL)
        {
            BM_CanStatusInfoTypeDef canStatus;
            BM_StatusTypeDef status = BM_GetStatus(channels[i], &canStatus);
            if (status & BM_ERROR_INITIALIZE)
            {
                // BM_Init() is not called yet.
                // Read our SDK documentation for details.
                DEMO_PRINT("BM_ERROR_INITIALIZE\n");
            }
            else if (status & BM_ERROR_ILLPARAMVAL)
            {
                // Channel handle is invalid.
                // Maybe it's not opened yet (using BM_OpenEx) or already closed?
                DEMO_PRINT("BM_ERROR_ILLPARAMVAL\n");
                BM_OpenEx(&channels[i], &channelRecoveryConfigs[i].info, channelRecoveryConfigs[i].mode, channelRecoveryConfigs[i].tres, &channelRecoveryConfigs[i].bitrate, &channelRecoveryConfigs[i].rxfilters[0], 1);
                channelRecoveryConfigs[i].previousRecoveryTs = currentTs;
            }
            else if (status & BM_ERROR_ILLOPERATION)
            {
                // USB Device operation failed.
                // Maybe the device is unplugged from host PC?
                
                DEMO_PRINT("BM_ERROR_ILLOPERATION\n");
                // Close all channels in the same device.
                uint64_t recoveredChannelMask = 0;
                for (int j = i; j < openedChannelCount; j++)
                {
                    BM_ChannelInfoTypeDef siblingInfo;
                    BM_GetChannelInfo(channels[j], &siblingInfo);
                    if (memcmp(channelRecoveryConfigs[i].info.sn, siblingInfo.sn, sizeof(siblingInfo.sn)) == 0 &&
                            memcmp(channelRecoveryConfigs[i].info.uid, siblingInfo.uid, sizeof(siblingInfo.uid)) == 0)
                    {
                        BM_Close(channels[j]);
                        channels[j] = NULL;
                        recoveredChannelMask |= 1ULL << j;
                    }
                }
                // Try reset device and remove device from opened device list kept by bmapi.
                //BM_ResetDevice(channels[i]);
                for (int j = i; j < openedChannelCount; j++)
                {
                    if (recoveredChannelMask & (1ULL << j))
                    {
                        BM_OpenEx(&channels[j], &channelRecoveryConfigs[j].info, channelRecoveryConfigs[j].mode, channelRecoveryConfigs[j].tres, &channelRecoveryConfigs[j].bitrate, &channelRecoveryConfigs[j].rxfilters[0], 1);
                        channelRecoveryConfigs[j].previousRecoveryTs = currentTs;
                    }
                }
            }
            else if (status & BM_ERROR_ANYBUSERR)
            {
                // BUSOFF
                // Maybe the remote CAN device is disconnected,
                // or you might want to check your bitrate, sample-position, tres configuration.
                if (i == port)
                {
                    DEMO_PRINT("BUSOFF RECOVERY\n");
                    BM_RecoverBusOff(channels[i]);
                    channelRecoveryConfigs[i].previousRecoveryTs = currentTs;
                }
            }
        }
    }
}

void Widget::on_rxAdvancedFilterCheck_clicked(bool checked)
{
    ui->rxDataMaskEdit->setEnabled(checked);
}



