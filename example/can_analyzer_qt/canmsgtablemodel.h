#ifndef CANMSGTABLEMODEL_H
#define CANMSGTABLEMODEL_H

#include <QAbstractTableModel>
#include "bmapi.h"

typedef struct MsgData
{
    BM_DataTypeDef msg;
    uint32_t port;
    uint64_t utcts;
} MsgDataTypeDef;

class CanMsgTableModel : public QAbstractTableModel
{
    Q_OBJECT

public:
    explicit CanMsgTableModel(QObject *parent = nullptr);

    // Header:
    QVariant headerData(int section, Qt::Orientation orientation, int role = Qt::DisplayRole) const override;

    // Basic functionality:
    int rowCount(const QModelIndex &parent = QModelIndex()) const override;
    int columnCount(const QModelIndex &parent = QModelIndex()) const override;

    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const override;

public:
    int insertMessage(const MsgDataTypeDef& msg);
    void clearAllMessages();

protected:
    QString msgId(const BM_DataTypeDef& msg) const;
    QString msgType(const BM_DataTypeDef& msg) const;
    uint32_t msgLength(const BM_DataTypeDef& msg) const;
    QString msgPayload(const BM_DataTypeDef& msg) const;

private:
    QList<MsgDataTypeDef> msgpool;

};

#endif // CANMSGTABLEMODEL_H
