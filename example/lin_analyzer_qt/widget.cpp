#include "widget.h"
#include "ui_widget.h"
#include <QMessageBox>
#include <QString>
#include <QCoreApplication>
#include <QtNetwork/QNetworkInterface>
#include <QDateTime>
#define SEND_COMBOBOX_NAME "txSendComboBox"
#define TYPE_COMBOBOX_NAME "txTypeComboBox"
#define CHECKSUMTYPE_COMBOBOX_NAME "txChecksumTypeComboBox"
#define ID_EDIT_NAME "txIdEdit"
#define CYCLE_EDIT_NAME "txCycleEdit"
#define LENGTH_EDIT_NAME "txLengthEdit"
#define COUNT_EDIT_NAME "txCountEdit"
#define DATA_EDIT_NAME "txDataEdit"
#define CHECKSUM_EDIT_NAME "txChecksumEdit"
#define Enable_Check "txEnableCheck"

#define MASTERTIMEOUT 100  //Unit in milliseconds
#define SLAVETIMEOUT  2000 //Unit in milliseconds
#define PAYLOADLENGTH 8

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

uint8_t calculatePID(uint8_t ucId)
{
    char chChecksum = ucId;
    char chMask[8], byte[8];
    memset(chMask, 0, sizeof(char) * 8);
    memset(byte, 0, sizeof(char) * 8);
    for (int nIndex = 0; nIndex < 8; nIndex++)
    {
        chMask[nIndex] = nIndex + 1;
        byte[nIndex] = ((chChecksum) >> nIndex) & 1;
    }
    
    char chP0, chP1;
    // chP0 = ID0+ID1+ID2+ID4
    chP0 = byte[0] ^ byte[1] ^ byte[2] ^ byte[4];
    
    
    // chP1 = ID1+ID3+ID4+ID5
    chP1 = byte[1] + byte[3] + byte[4] + byte[5];
    
    chP1 = ~chP1;
    
    //00010110 -> p00010110
    chP0 = chP0 << 6;
    chP1 = chP1 << 7;
    chChecksum = chChecksum + chP0 + chP1;
    ucId = chChecksum;
    return ucId;
}

uint8_t calculateChecksum(const uint8_t* xData, uint8_t xDataLen, uint8_t pId)
{
    uint16_t sum = pId;
    for (uint8_t i = 0; i < xDataLen; ++i)
    {
        sum += xData[i];
        while (sum > 0xFF)
        {
            sum -= 0xFF;
        }
    }
    return static_cast<uint8_t>((~sum) & 0xff);
}

bool sanitizePayload(QString& payload)
{
    QString sanitizedPayload;
    bool isEffective = true;
    for (QChar c : payload)
    {
        if (c == ' ' || c.isDigit() || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
        {
            sanitizedPayload.append(c);
        }
        else
        {
            isEffective = false;
        }
    }
    payload = sanitizedPayload;
    return isEffective;
}

Widget::Widget(QWidget* parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
    , msgTableModel(NULL)
    , openedChannelCount(0)
{
    memset(channels, 0, sizeof(channels));
    ui->setupUi(this);
    ui->groupBox_rxFilter->setEnabled(true);
    ui->groupBox_txTable->setEnabled(false);
    ui->modeCombo->clear();
    ui->modeCombo->addItem(tr("Master"), (int)BM_LIN_MASTER_MODE);
    ui->modeCombo->addItem(tr("Slave"), (int)BM_LIN_SLAVE_MODE);
    msgIndex = 0;
    msgTableModel = new LinMsgTableModel;
    ui->msgTableView->setModel(msgTableModel);
    ui->msgTableView->horizontalHeader()->setSectionResizeMode(ui->msgTableView->horizontalHeader()->count() - 1, QHeaderView::Stretch);

    int columnIndex = ui->msgTableView->horizontalHeader()->count() - 5;
    ui->msgTableView->horizontalHeader()->setSectionResizeMode(columnIndex, QHeaderView::Fixed);
    ui->msgTableView->horizontalHeader()->resizeSection(columnIndex, 150);

    columnIndex = ui->msgTableView->horizontalHeader()->count() - 2;
    ui->msgTableView->horizontalHeader()->setSectionResizeMode(columnIndex, QHeaderView::Fixed);
    ui->msgTableView->horizontalHeader()->resizeSection(columnIndex, 250);

    ui->msgTableView->horizontalHeader()->setSectionResizeMode(1, QHeaderView::Fixed);
    ui->msgTableView->horizontalHeader()->resizeSection(1, 200);
    validator = new QRegExpValidator(QRegExp("[0-9a-fA-F]+"), this);
    rxTimer = new QTimer(this);
    getAvailableHostIpList();
    connect(rxTimer, &QTimer::timeout, this, &Widget::readPendingMessages);
    
    connect(ui->txIdEdit_1, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    connect(ui->txIdEdit_2, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    connect(ui->txIdEdit_3, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    connect(ui->txIdEdit_4, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    connect(ui->txIdEdit_5, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    connect(ui->txIdEdit_6, &QLineEdit::editingFinished, this, &Widget::checksumChanged);
    
    idLineEdits.append(ui->txIdEdit_1);
    idLineEdits.append(ui->txIdEdit_2);
    idLineEdits.append(ui->txIdEdit_3);
    idLineEdits.append(ui->txIdEdit_4);
    idLineEdits.append(ui->txIdEdit_5);
    idLineEdits.append(ui->txIdEdit_6);
    
    int i = 0;
    for (QLineEdit* lineEdit : idLineEdits)
    {
        previousID[i] = lineEdit->text();
        i++;
    }
    
    connect(ui->txDataEdit_1, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    connect(ui->txDataEdit_2, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    connect(ui->txDataEdit_3, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    connect(ui->txDataEdit_4, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    connect(ui->txDataEdit_5, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    connect(ui->txDataEdit_6, &QLineEdit::textChanged, this, &Widget::checksumChanged);
    
    connect(ui->txChecksumTypeComboBox_1, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    connect(ui->txChecksumTypeComboBox_2, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    connect(ui->txChecksumTypeComboBox_3, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    connect(ui->txChecksumTypeComboBox_4, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    connect(ui->txChecksumTypeComboBox_5, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    connect(ui->txChecksumTypeComboBox_6, &QComboBox::currentTextChanged, this, &Widget::checksumChanged);
    
    connect(ui->txLengthEdit_1, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_1, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_2, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_3, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_4, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_5, &QLineEdit::textChanged, this, &Widget::checktxLength);
    connect(ui->txLengthEdit_6, &QLineEdit::textChanged, this, &Widget::checktxLength);
    
    connect(ui->txCycleEdit_1, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    connect(ui->txCycleEdit_2, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    connect(ui->txCycleEdit_3, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    connect(ui->txCycleEdit_4, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    connect(ui->txCycleEdit_5, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    connect(ui->txCycleEdit_6, &QLineEdit::editingFinished, this, &Widget::checktxCycle);
    
    connect(ui->txCountEdit_1, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    connect(ui->txCountEdit_2, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    connect(ui->txCountEdit_3, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    connect(ui->txCountEdit_4, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    connect(ui->txCountEdit_5, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    connect(ui->txCountEdit_6, &QLineEdit::editingFinished, this, &Widget::checktxCount);
    
    connect(ui->txTypeComboBox_1, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    connect(ui->txTypeComboBox_2, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    connect(ui->txTypeComboBox_3, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    connect(ui->txTypeComboBox_4, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    connect(ui->txTypeComboBox_5, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    connect(ui->txTypeComboBox_6, static_cast<void (QComboBox::*)(int)>(&QComboBox::currentIndexChanged), this, &Widget::typeChanged);
    
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
    if (n <= 8)
    {
        dlc = n;
    }
    else
    {
        dlc = 8;
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
            int nlin = 0;
            for (int i = 0; i < n; i++)
            {
                if (remotechannelinfos[i].cap & BM_LIN_CAP)
                {
                    channelinfos[nlin++] = remotechannelinfos[i];
                }
            }
            n = nlin;
            succeed = true;
        }
        delete[] remotechannelinfos;
    }
    else
    {
        if (BM_ERROR_OK == BM_EnumerateByCap(channelinfos, &n, BM_LIN_CAP))
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
    if (ui->channelCombo->count())
    {
        QList<QComboBox*> comboBoxList = ui->groupBox_txTable->findChildren<QComboBox*>();
        for (auto& it : comboBoxList)
        {
            QString str = it->objectName();
            if (str.indexOf("txSendComboBox") >= 0 || str.indexOf("txTypeComboBox") >= 0)
            {
                it->clear();
            }
        }
        if (checked)
        {
            openedChannelCount = 0;
            uint64_t filterValue = 0;
            QList<QCheckBox*> checkBoxList = ui->groupBox_rxFilter->findChildren<QCheckBox*>();
            for (auto& it : checkBoxList)
            {
                if (it->isChecked())
                {
                    filterValue |= 1ULL << it->text().toUInt();
                }
            }
            
            BM_LinModeTypeDef mode = (BM_LinModeTypeDef)ui->modeCombo->currentData().value<int>();
            int i = 0;
            for (auto& it : comboBoxList)
            {
                QString str = it->objectName();
                if (str.indexOf("txTypeComboBox") >= 0)
                {
                    QString str = QString("_%1").arg(i + 1);
                    QLineEdit* txCycle = ui->groupBox_txTable->findChild<QLineEdit*>(CYCLE_EDIT_NAME + str);
                    if (mode == BM_LIN_MASTER_MODE)
                    {
                        txCycle->setText("1000");
                        txCycle->setEnabled(true);
                        it->addItem(tr("Master Write"), (int)LIN_MASTER_WRITE);
                        it->addItem(tr("Master Read"), (int)LIN_MASTER_READ);
                    }
                    else if (mode == BM_LIN_SLAVE_MODE)
                    {
                        txCycle->setText("");
                        txCycle->setEnabled(false);
                        it->addItem(tr("Slave Write"), (int)LIN_SLAVE_WRITE);
                    }
                    i++;
                }
            }
            for (int i = 0; i < ui->channelCombo->count(); i++)
            {
                if (ui->channelCombo->isChecked(i) == true)
                {
                    BM_ChannelInfoTypeDef* channelinfo = &channelinfos[i]; // Iterate all enumerated ports here
                    for (auto& it : comboBoxList)
                    {
                        QString str = it->objectName();
                        if (str.indexOf("txSendComboBox") >= 0)
                        {
                            it->addItem(QString::number(openedChannelCount + 1)); // Show opened port index here
                        }
                    }
                    BM_TerminalResistorTypeDef tres = ui->tresCheck->isChecked() ? BM_TRESISTOR_PULLUP_1K : BM_TRESISTOR_DISABLED;
                    BM_LinVoltageTypeDef voltage = ui->outputCheck->isChecked() ? BM_LIN_VOLTAGE_12V_OUT : BM_LIN_VOLTAGE_12V_IN;
                    BM_RxFilterTypeDef rxfilter;
                    {
                        // Get the Filter
                        memset(&rxfilter, 0, sizeof(rxfilter));
                        rxfilter.id_mask = (uint32_t)(filterValue >> 32);
                        rxfilter.id_value = (uint32_t)filterValue;
                        rxfilter.type = BM_RXFILTER_BASIC;
                    }
                    
                    BM_BitrateTypeDef bitrate;
                    {
                        memset(&bitrate, 0, sizeof(bitrate));
                        bitrate.nbitrate = (uint16_t)ui->nominalBitrateEdit->text().toUInt();
                    }
                    BM_StatusTypeDef status = BM_OpenEx(&channels[openedChannelCount], channelinfo, mode, tres, &bitrate, &rxfilter, 1);
                    BM_SetLinVoltage(channels[openedChannelCount], voltage);
                    msgIndex = 0;
                    if (status != BM_ERROR_OK)
                    {
                        char buffer[1024];
                        BM_GetErrorText(status, buffer, sizeof(buffer), 0);
                        QString prompt = QString(tr("Failed to open channel %1, error=%2(%3), please plugin your Busmust device and press 'Enumerate'."))
                                .arg(channelinfo->name)
                                .arg((int)status, 8, 16)
                                .arg(buffer);
                        QMessageBox::critical(this, tr("Error"), prompt);
                        success = false;
                    }
                    else
                    {
                        openedChannelCount++;
                    }
                    BM_SetPtpMode(channels[openedChannelCount], BM_PTP_INPUT_USB_SOF);
                }
            }
            // Sync PTP timestamp for all devices
            if (BM_SyncPtpTimes(&channels[0], openedChannelCount) != BM_ERROR_OK)
            {
                qWarning("Failed to sync PTP timestamps.\n");
            }
        }
        
        if (checked && success)
        {
            msgTableModel->clearAllMessages();
            rxTimer->start(5);
            ui->openButton->setText(tr("Close"));
            ui->outputCheck->setEnabled(false);
            ui->modeCombo->setEnabled(false);
            ui->tresCheck->setEnabled(false);
            ui->nominalBitrateEdit->setEnabled(false);
            ui->groupBox_rxFilter->setEnabled(false);
            ui->groupBox_txTable->setEnabled(true);
            ui->enumerateButton->setEnabled(false);
            ui->txButton->setEnabled(true);
            ui->txButton->setText(tr("Transmit"));
            ui->txButton->setChecked(false);
            ui->channelCombo->setEnabled(false);
            stopSendMsg();
        }
        else
        {
            rxTimer->stop();
            ui->openButton->setChecked(false);
            ui->openButton->setText(tr("Open"));
            ui->outputCheck->setEnabled(true);
            ui->modeCombo->setEnabled(true);
            ui->tresCheck->setEnabled(true);
            ui->groupBox_rxFilter->setEnabled(true);
            ui->groupBox_txTable->setEnabled(false);
            ui->nominalBitrateEdit->setEnabled(true);
            ui->enumerateButton->setEnabled(true);
            ui->txButton->setEnabled(false);
            ui->txButton->setText(tr("Transmit"));
            ui->channelCombo->setEnabled(true);
            stopSendMsg();
            for (int i = 0; i < openedChannelCount; i++)
            {
                BM_Close(channels[i]);
                channels[i] = nullptr;
            }
            openedChannelCount = 0;
        }
    }
    else
    {
        ui->openButton->setCheckable(true);
        ui->openButton->setChecked(false);
    }
}

void Widget::on_enumerateButton_clicked()
{
    enumerate();
}


void Widget::readPendingMessages(void)
{
    BM_DataTypeDef msg;
    bool newMsgInserted = false;
    int state = 0;
    for (int i = 0; i < openedChannelCount; i++)
    {
        if (channels[i] && msgTableModel)
        {
            while (BM_Read(channels[i], &msg) == BM_ERROR_OK)
            {
                BM_LinModeTypeDef mode = (BM_LinModeTypeDef)ui->modeCombo->currentData().value<int>();
                BM_LinMessageTypeDef* linMessage = (BM_LinMessageTypeDef*)msg.payload;
                if (linMessage->ctrl.lin.ERRORS & BM_LIN_TRANSMIT_TIMEOUT)
                {
                    state = LIN_TIMEOUT_ERROR;
                }
                if (linMessage->ctrl.lin.ISTXTASK)
                {
                    if(msg.header.type & BM_ACK_DATA)
                    {
                        int totalCount = 0;
                        bool countChanges = false;
                        for (MsgTxtask& value : txTaskMsgList[i])
                        {
                            totalCount += value.txTask.nrounds;
                            if (value.txTask.lin.ID == linMessage->id)
                            {
                                value.txTask.nrounds--;
                                countChanges = true;
                            }
                        }
                        if (countChanges)
                        {
                            totalCount--;
                            if (totalCount <= 0)
                            {
                                on_txButton_clicked(false);
                            }
                        }
                    }
                    else
                    {
                        state |= LIN_MASTER_READ_ERROR;
                    }
                }
                uint8_t pid = 0;
                if (linMessage->ctrl.lin.ENHANCED_CHECKSUM)
                {
                    pid = calculatePID(linMessage->id);
                }
                int checksum = calculateChecksum(linMessage->payload, linMessage->ctrl.lin.DLC, pid);
                if (checksum != linMessage->ctrl.lin.CHECKSUM)
                {
                    state |= LIN_CHECKSUM_ERROR;
                }
                MsgDataTypeDef data;
                data.msg = msg;
                data.port = i + 1;
                data.state = state;
                data.mode = (int)mode;
                data.index = msgIndex;
                if (BM_GetDataPtpTimestamp(channels[i], &msg, &data.utcts) != BM_ERROR_OK)
                {
                    // If failed to extract a PTP timestamp from data, use current time on host-machine.
                    data.utcts = BM_GetHostPtpTime();
                }
                msgIndex++;
                msgTableModel->insertMessage(data);
                ui->msgTableView->scrollToBottom();
                newMsgInserted = true;

            }
        }
    }
    if (newMsgInserted)
    {
        ui->msgTableView->scrollToBottom();
    }
}

void Widget::writePendingMessages(void)
{
}



void Widget::checksumChanged()
{
    QString str = sender()->objectName().right(2);
    QString name = sender()->objectName();
    QComboBox* txChecksumType = ui->groupBox_txTable->findChild<QComboBox*>(CHECKSUMTYPE_COMBOBOX_NAME + str);
    QLineEdit* txId = ui->groupBox_txTable->findChild<QLineEdit*>(ID_EDIT_NAME + str);
    QLineEdit* txData = ui->groupBox_txTable->findChild<QLineEdit*>(DATA_EDIT_NAME + str);
    QLineEdit* txChecksum = ui->groupBox_txTable->findChild<QLineEdit*>(CHECKSUM_EDIT_NAME + str);
    QLineEdit* txDlc = ui->groupBox_txTable->findChild<QLineEdit*>(LENGTH_EDIT_NAME + str);
    int checksumType = txChecksumType->currentIndex() + 1;
    int id = txId->text().toUInt(NULL, 16);
    int index = str.right(1).toUInt() - 1;
    if (name.contains("txIdEdit"))
    {
        QString formattedText = QString("%1").arg(id, 2, 16, QChar('0')).toUpper();
        if (!(id < 64 && id >= 0))
        {
            QMessageBox::warning(nullptr, "input error", "Please enter an integer between 0x00 and 0x3F");
            id = previousID[index].toUInt(NULL, 16);
            formattedText = QString("%1").arg(id, 2, 16, QChar('0')).toUpper();
            txId->setText(formattedText);
            return;
        }
        for (QLineEdit* otherLineEdit : idLineEdits)
        {
            if (otherLineEdit != txId)
            {
                int other = otherLineEdit->text().toUInt(NULL, 16);
                if (other == id)
                {
                    int ntxtask = otherLineEdit->objectName().right(1).toUInt();
                    QMessageBox::warning(nullptr, "Warning", QString("LIN id %1 is already used by TxTask %2.").arg(id, 0, 16).arg(ntxtask), QMessageBox::Ok);
                    id = previousID[index].toUInt(NULL, 16);
                    formattedText = QString("%1").arg(id, 2, 16, QChar('0')).toUpper();
                    txId->setText(formattedText);
                    return;
                }
            }
        }
        previousID[index] = formattedText;
        txId->setText(formattedText);
    }
    
    QString payload = txData->text();
    
    if (!sanitizePayload(payload))
    {
        txData->setText(payload);
    }
    payload = payload.remove(QLatin1Char(' '));
    int j = 0;
    char payloadValue[PAYLOADLENGTH];
    memset(payloadValue, 0, sizeof(char) * PAYLOADLENGTH);
    while (j * 2 < payload.length() && j < PAYLOADLENGTH)
    {
        payloadValue[j] = payload.mid(j * 2, 2).toUInt(NULL, 16);
        j++;
    }
    
    int dlc = txDlc->text().toInt();
    if (dlc != j)
    {
        txDlc->blockSignals(true);
        txDlc->setText(QString::number(j));
        txDlc->blockSignals(false);
    }
    
    
    uint8_t pid = 0;
    if (checksumType == BM_LIN_ENHANCED_CHECKSUM)
    {
        pid = calculatePID(id);
    }
    int checksum = calculateChecksum((uint8_t*)payloadValue, j, pid);
    QString formattedText = QString("%1").arg(checksum, 2, 16, QChar('0')).toUpper();
    txChecksum->setText(formattedText);
    formattedText = QString("%1").arg(id, 2, 16, QChar('0')).toUpper();
}

void Widget::checktxLength()
{
    QString str = sender()->objectName().right(2);
    QLineEdit* txData = ui->groupBox_txTable->findChild<QLineEdit*>(DATA_EDIT_NAME + str);
    QLineEdit* txDlc = ui->groupBox_txTable->findChild<QLineEdit*>(LENGTH_EDIT_NAME + str);
    QString text = txDlc->text();
    int num = text.toInt();
    if (num > 8)
    {
        QMessageBox::warning(nullptr, "input error", "Please enter an integer between 0 and 8");
        txDlc->setText("1");
    }
    else
    {
        QString payload = txData->text().remove(QLatin1Char(' '));
        int j = 0;
        char payloadValue[PAYLOADLENGTH];
        memset(payloadValue, 0, sizeof(char) * PAYLOADLENGTH);
        while (j * 2 < num * 2 && j < PAYLOADLENGTH)
        {
            payloadValue[j] = payload.mid(j * 2, 2).toUInt(NULL, 16);
            j++;
        }
        
        QString formattedString = "";
        for (int i = 0; i < j; ++i)
        {
            formattedString.append(QString("%1 ").arg(static_cast<unsigned char>(payloadValue[i]), 2, 16, QChar('0')));
        }
        txData->setText(formattedString);
    }
}

void Widget::checktxCycle()
{
    QString str = sender()->objectName().right(2);
    QLineEdit* txCycle = ui->groupBox_txTable->findChild<QLineEdit*>(CYCLE_EDIT_NAME + str);
    QString text = txCycle->text();
    int num = text.toInt();
    if (!(num >= 1 && num <= 60000)) {
        QMessageBox::warning(nullptr, "input error", "Please enter an integer between 1 and 6000");
        txCycle->setText("1000");
    }
}

void Widget::checktxCount()
{
    QString str = sender()->objectName().right(2);
    QLineEdit* txCount = ui->groupBox_txTable->findChild<QLineEdit*>(COUNT_EDIT_NAME + str);
    QString text = txCount->text();
    int num = text.toInt();
    if (num == 0)
    {
        txCount->setText("INF");
    }
    else if (!(num >= 1 && num <= 65534))
    {
        QMessageBox::warning(nullptr, "input error", "Please enter an integer between 1 and 65534");
        txCount->setText("10");
    }
}

void Widget::typeChanged(int)
{
    QString str = sender()->objectName().right(2);
    QComboBox* txType = ui->groupBox_txTable->findChild<QComboBox*>(TYPE_COMBOBOX_NAME + str);
    QLineEdit* txChecksum = ui->groupBox_txTable->findChild<QLineEdit*>(CHECKSUM_EDIT_NAME + str);
    QComboBox* txChecksumType = ui->groupBox_txTable->findChild<QComboBox*>(CHECKSUMTYPE_COMBOBOX_NAME + str);
    QLineEdit* txData = ui->groupBox_txTable->findChild<QLineEdit*>(DATA_EDIT_NAME + str);
    QLineEdit* txDlc = ui->groupBox_txTable->findChild<QLineEdit*>(LENGTH_EDIT_NAME + str);
    if (txType->currentData().value<int>() == LIN_MASTER_READ)
    {
        txData->setText("00 00 00 00 00 00 00 00");
        txData->setEnabled(false);
        txChecksum->setEnabled(false);
        txChecksumType->setCurrentIndex(0);
        txChecksumType->setEnabled(false);
        txDlc->setEnabled(false);
    }
    else
    {
        txData->setEnabled(true);
        txChecksum->setEnabled(true);
        txChecksumType->setEnabled(true);
        txDlc->setEnabled(true);
        checksumChanged();
    }
}


void Widget::on_selectAllButton_clicked()
{
    QList<QCheckBox*> checkBoxList = ui->groupBox_rxFilter->findChildren<QCheckBox*>();
    for (auto& it : checkBoxList)
    {
        it->setChecked(true);
    }
}

void Widget::on_reverseButton_clicked()
{
    QList<QCheckBox*> checkBoxList = ui->groupBox_rxFilter->findChildren<QCheckBox*>();
    for (auto& it : checkBoxList)
    {
        it->setChecked(!it->isChecked());
    }
}

void Widget::stopSendMsg()
{
    for (int i = 0; i < ui->channelCombo->count(); i++)
    {
        BM_TxTaskTypeDef tasks[NTXTASK];
        memset(&tasks, 0, sizeof(BM_TxTaskTypeDef) * NTXTASK);
        int j = 0;
        Q_FOREACH(MsgTxtask value, txTaskMsgList[i])
        {
            tasks[j] = value.txTask;
            tasks[j].type = 0;
            j++;
        }
        BM_SetTxTasks(channels[i], (BM_TxTaskTypeDef*)tasks, NTXTASK);
        txTaskMsgList[i].clear();
    }
}

void Widget::on_txButton_clicked(bool checked)
{
    if (checked)
    {
        bool success = false;
        for (int i = 0; i < NCHANNEL; i++)
        {
            txTaskMsgList[i].clear();
        }
        for (int i = 0; i < NTXTASK; i++)
        {
            QString str = QString("_%1").arg(i + 1);
            QCheckBox* txData = ui->groupBox_txTable->findChild<QCheckBox*>(Enable_Check + str);
            if (txData->isChecked())
            {
                QComboBox* txSend = ui->groupBox_txTable->findChild<QComboBox*>(SEND_COMBOBOX_NAME + str);
                QComboBox* txType = ui->groupBox_txTable->findChild<QComboBox*>(TYPE_COMBOBOX_NAME + str);
                QComboBox* txChecksumType = ui->groupBox_txTable->findChild<QComboBox*>(CHECKSUMTYPE_COMBOBOX_NAME + str);
                
                QLineEdit* txId = ui->groupBox_txTable->findChild<QLineEdit*>(ID_EDIT_NAME + str);
                QLineEdit* txLength = ui->groupBox_txTable->findChild<QLineEdit*>(LENGTH_EDIT_NAME + str);
                QLineEdit* txCycle = ui->groupBox_txTable->findChild<QLineEdit*>(CYCLE_EDIT_NAME + str);
                QLineEdit* txCount = ui->groupBox_txTable->findChild<QLineEdit*>(COUNT_EDIT_NAME + str);
                QLineEdit* txData = ui->groupBox_txTable->findChild<QLineEdit*>(DATA_EDIT_NAME + str);
                int port = txSend->currentIndex();
                if (port >= 0 && checked && channels[port] && port < int(sizeof(channels) / sizeof(channels[0])))
                {
                    MsgTxtask msg;
                    memset(&msg, 0, sizeof(msg));
                    msg.type = txType->currentData().value<int>();
                    BM_TxTaskTypeDef txTask;
                    memset(&txTask, 0, sizeof(txTask));
                    uint id = txId->text().toUInt(NULL, 16);
                    uint length = txLength->text().toUInt();
                    int dlc = lengthToDlc(length);
                    memset(txTask.payload, 0, sizeof(txTask.payload));
                    QString payload = txData->text().remove(QLatin1Char(' '));
                    int j = 0;
                    if (msg.type == LIN_MASTER_READ)
                    {
                        txData->setText("00 00 00 00 00 00 00 00");
                    }
                    else
                    {
                        txTask.flags = BM_LIN_MESSAGE_FLAGS_TRANSMIT;
                        while (j * 2 < payload.length())
                        {
                            txTask.payload[j] = payload.mid(j * 2, 2).toUInt(NULL, 16);
                            j++;
                        }
                        payload = "";
                        QString formattedString = "";
                        for(int i = 0; i < j; i++)
                        {
                            formattedString = formattedString.sprintf("%02x", txTask.payload[i]);
                            if (i == 0)
                            {
                                payload += formattedString;
                            }
                            else
                            {
                                payload = payload + " " + formattedString;
                            }
                            
                        }
                        txData->setText(payload);
                    }
                    int checksumType = txChecksumType->currentIndex() + 1;
                    uint8_t pid = 0;
                    if (checksumType == BM_LIN_ENHANCED_CHECKSUM)
                    {
                        pid = calculatePID(id);
                        txTask.flags |= BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM;
                    }
                    txTask.lin.CHECKSUM = calculateChecksum(txTask.payload, dlc, pid);
                    txTask.lin.ID = id;
                    txTask.length = dlc;
                    txTask.cycle = txCycle->text().toUInt();
                    txTask.type = BM_TXTASK_FIXED;
                    txTask.nmessages = 1;
                    if (txCount->text() == "INF")
                    {
                        txTask.nrounds = 0xFFFF;
                    }
                    else
                    {
                        txTask.nrounds = txCount->text().toUInt();
                    }
                    msg.txTask = txTask;
                    msg.txTaskCount = txTask.nrounds;
                    msg.channel = port;
                    txTaskMsgList[port].append(msg);
                    success = true;
                }
            }
        }
        if (success)
        {
            for (int i = 0; i < openedChannelCount; i++)
            {
                BM_TxTaskTypeDef tasks[NTXTASK];
                memset(&tasks, 0, sizeof(BM_TxTaskTypeDef) * NTXTASK);
                int j = 0;
                Q_FOREACH(MsgTxtask value, txTaskMsgList[i])
                {
                    tasks[j] = value.txTask;
                    j++;
                }
                //BM_ClearBuffer(channels[i]);
                BM_SetTxTasks(channels[i], (BM_TxTaskTypeDef*)tasks, NTXTASK);
            }
            ui->txButton->setText(tr("Stop"));
            ui->groupBox_txTable->setEnabled(false);
        }
    }
    else
    {
        ui->txButton->setChecked(false);
        ui->txButton->setText(tr("Transmit"));
        ui->groupBox_txTable->setEnabled(true);
        stopSendMsg();
    }
}

