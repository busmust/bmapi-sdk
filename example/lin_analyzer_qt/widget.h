#ifndef WIDGET_H
#define WIDGET_H

#include <QWidget>
#include <QTimer>
#include "bmapi.h"
#include "linmsgtablemodel.h"
#include "mycombobox.h"
#include <QDebug>
#include <QPushButton>
#define NTXTASK       6
#define NCHANNEL      24

enum LIN_MSG_TYPE
{
    LIN_RECEIVE = 0,
    LIN_MASTER_WRITE,
    LIN_MASTER_READ,
    LIN_SLAVE_WRITE,
};

typedef struct MsgTxtask
{
    BM_TxTaskTypeDef txTask;
    int txTaskCount;
    int channel;
    int type;
} MsgTxtaskTypeDef;

QT_BEGIN_NAMESPACE
namespace Ui { class Widget; }
QT_END_NAMESPACE

class Widget : public QWidget
{
    Q_OBJECT

public:
    Widget(QWidget* parent = nullptr);
    ~Widget();
    int lengthToDlc(int length);

protected:
    void enumerate();

    void getAvailableHostIpList() const;

private slots:
    void readPendingMessages(void);

    void writePendingMessages(void);

    void checksumChanged();

    void checktxLength();

    void checktxCycle();

    void checktxCount();

    void typeChanged(int index);

    void on_openButton_clicked(bool checked);

    void on_enumerateButton_clicked();

    void on_selectAllButton_clicked();

    void on_txButton_clicked(bool checked);

    void on_reverseButton_clicked();

    void stopSendMsg();

private:
    Ui::Widget* ui;

protected:
    BM_ChannelHandle channels[32];
    BM_ChannelInfoTypeDef channelinfos[32];
    LinMsgTableModel* msgTableModel;
    QTimer* rxTimer;
    QList <MsgTxtask> txTaskMsgList[NCHANNEL];
    int openedChannelCount;
    QRegExpValidator* validator;
    QVector<QLineEdit*> idLineEdits;
    QString previousID[NTXTASK];
    int msgIndex;
};
#endif // WIDGET_H
