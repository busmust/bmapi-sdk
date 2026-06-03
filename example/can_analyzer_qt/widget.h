#ifndef WIDGET_H
#define WIDGET_H

#include <QWidget>
#include <QTimer>
#include "bmapi.h"
#include "canmsgtablemodel.h"
#include "mycombobox.h"
#include <QDebug>
QT_BEGIN_NAMESPACE
namespace Ui { class Widget; }
QT_END_NAMESPACE

class Widget : public QWidget
{
    Q_OBJECT

public:
    Widget(QWidget *parent = nullptr);
    ~Widget();
    int lengthToDlc(int length);

    void getAvailableHostIpList() const;
protected:
    void enumerate();

private slots:
    void readPendingMessages(void);

    void writePendingMessages(void);

    void recoverFromError(int port = -1);

    void on_openButton_clicked(bool checked);

    void on_enumerateButton_clicked();

    void on_txButton_clicked(bool checked);

    void on_rxAdvancedFilterCheck_clicked(bool checked);

private:
    Ui::Widget *ui;

protected:
    int openedChannelCount;
    BM_ChannelHandle channels[32];
    struct  
    {
        uint64_t previousRecoveryTs;
        BM_ChannelInfoTypeDef info;
        BM_CanModeTypeDef mode;
        BM_BitrateTypeDef bitrate;
        BM_TerminalResistorTypeDef tres;
        BM_RxFilterTypeDef rxfilters[2];
        BM_TxTaskTypeDef txtasks[64];
    } channelRecoveryConfigs[32];
    BM_ChannelInfoTypeDef channelinfos[32];
    CanMsgTableModel* msgTableModel;
    QTimer* rxTimer;
    QTimer* txTimer;
    QTimer* recoverFromErrorTimer;
    BM_DataTypeDef pendingTxMsg;
    int pendingTxMsgCount;

};
#endif // WIDGET_H
