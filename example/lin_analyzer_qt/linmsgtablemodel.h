#ifndef CANMSGTABLEMODEL_H
#define CANMSGTABLEMODEL_H

#include <QAbstractTableModel>
#include "bmapi.h"

enum LIN_MSG_STATE
{
    LIN_OK_ERROR = 0x0,
    LIN_TIMEOUT_ERROR = 0x1,
    LIN_CHECKSUM_ERROR = 0x2,
    LIN_MASTER_READ_ERROR = 0x4,
};

typedef struct MsgData
{
    BM_DataTypeDef msg;
    uint32_t port;
    int state;
    int mode;
    int index;
    uint64_t utcts;
} MsgDataTypeDef;

class LinMsgTableModel : public QAbstractTableModel
{
    Q_OBJECT

public:
    explicit LinMsgTableModel(QObject* parent = nullptr);

    // Header:
    QVariant headerData(int section, Qt::Orientation orientation, int role = Qt::DisplayRole) const override;

    // Basic functionality:
    int rowCount(const QModelIndex& parent = QModelIndex()) const override;
    int columnCount(const QModelIndex& parent = QModelIndex()) const override;

    QVariant data(const QModelIndex& index, int role = Qt::DisplayRole) const override;

public:
    void clearAllMessages();
    int insertMessage(const MsgDataTypeDef& msg);
protected:
    QString msgIndex(const MsgDataTypeDef& data) const;
    QString msgId(const BM_DataTypeDef& msg) const;
    QString msgChecksum(const BM_DataTypeDef &msg, const bool timeout) const;
    QString msgResult(const MsgData& msg) const;
    QString msgPayload(const BM_DataTypeDef &msg, const bool timeout) const;
    QString msgLength(const BM_DataTypeDef &msg, const bool timeout) const;
    QString msgType(const MsgDataTypeDef& data) const;
private:
    QList<MsgDataTypeDef> msgpool;
    int msgMode = 0;

};

#endif // CANMSGTABLEMODEL_H
