#include "linmsgtablemodel.h"
#include <time.h>
void timeval_to_formatted_string(uint64_t utcTimestamp, char* str, size_t size)
{
    time_t time = (utcTimestamp / 1000000000);
    struct tm* tm_info;
    tm_info = localtime(&time);

    if (tm_info == NULL)
    {
        snprintf(str, size, "Invalid");
        return;
    }
    else
    {
        strftime(str, size, "%Y-%m-%d %H:%M:%S:", tm_info);

        snprintf(str + strlen(str), size - strlen(str), "%06ld", (long)((utcTimestamp % 1000000000) / 1000));
    }
}

LinMsgTableModel::LinMsgTableModel(QObject* parent)
    : QAbstractTableModel(parent)
{
}

QVariant LinMsgTableModel::headerData(int section, Qt::Orientation orientation, int role) const
{
    if (orientation == Qt::Horizontal && role == Qt::DisplayRole)
    {
        switch (section)
        {
        case 0: return QString("Index"); break;
        case 1: return QString("UtcTime (us)"); break;
        case 2: return QString("Time (s)"); break;
        case 3: return QString("TX/RX"); break;
        case 4: return QString("Channel"); break;
        case 5: return QString("ID (HEX)"); break;
        case 6: return QString("Type"); break;
        case 7: return QString("Length (B)"); break;
        case 8: return QString("Checksum"); break;
        case 9: return QString(" Result "); break;
        case 10: return QString("Payload"); break;
        default: break;
        }
    }

    return QVariant();
}

int LinMsgTableModel::rowCount(const QModelIndex& parent) const
{
    if (parent.isValid())
        return 0;

    return msgpool.size();
}

int LinMsgTableModel::columnCount(const QModelIndex& parent) const
{
    if (parent.isValid())
        return 0;

    return 11;
}

QString LinMsgTableModel::msgId(const BM_DataTypeDef& msg) const
{
    const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)msg.payload;
    uint32_t MsgId = linMsg->id;
    return (QString("%1").arg(MsgId, 2, 16, QChar('0'))).toUpper();
}

QString LinMsgTableModel::msgType(const MsgDataTypeDef& data) const
{
    QString type;
    if (data.state == LIN_MASTER_READ_ERROR)
    {
        type = "Master Read";
    }
    else
    {
        if (data.mode == BM_LIN_MASTER_MODE)
        {
            type += "Master ";
        }
        else if (data.mode == BM_LIN_SLAVE_MODE)
        {
            type += "Slave ";
        }
        const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)data.msg.payload;
        type += QString((linMsg->ctrl.lin.TRANSMIT) ? "Write" : "Read");
    }

    return type;
}

QString LinMsgTableModel::msgChecksum(const BM_DataTypeDef& msg, const bool timeout) const
{
    const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)msg.payload;
    if (!timeout)
    {
        return (QString("%1").arg(linMsg->ctrl.lin.CHECKSUM, 2, 16, QChar('0'))).toUpper();
    }
    else
    {
        return "";
    }

}

QString LinMsgTableModel::msgResult(const MsgData &msg) const
{
    const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)msg.msg.payload;
    QString str;
    if (msg.state & LIN_TIMEOUT_ERROR)
    {
        str = "No Data(Timeout)";
    }
    else if(msg.state == LIN_OK_ERROR)
    {
        if (linMsg->ctrl.lin.ENHANCED_CHECKSUM)
        {
            str = "Enhanced(Successful)";
        }
        else
        {
            str = "Classic(Successful)";
        }
    }
    else if(msg.state == LIN_CHECKSUM_ERROR)
    {
        if (linMsg->ctrl.lin.ENHANCED_CHECKSUM)
        {
            str = "Enhanced checksum error";
        }
        else
        {
            str = "Classic checksum error";
        }
    }
    return str;
}

QString LinMsgTableModel::msgPayload(const BM_DataTypeDef& msg, const bool timeout) const
{
    const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)msg.payload;
    uint32_t length = linMsg->ctrl.lin.DLC;
    QString payload = "";
    if (!timeout)
    {
        uint32_t i;
        for (i = 0; i < length; i++)
        {
            payload += QString("%1 ").arg(linMsg->payload[i], 2, 16, QLatin1Char('0'));
        }
    }
    return payload.toUpper();
}

QString LinMsgTableModel::msgLength(const BM_DataTypeDef& msg, const bool timeout) const
{
    const BM_LinMessageTypeDef* linMsg = (const BM_LinMessageTypeDef*)msg.payload;
    uint32_t length = linMsg->ctrl.lin.DLC;
    if (!timeout)
    {
        return QString::number(length).toUpper();
    }
    else
    {
        return "";
    }
}

QVariant LinMsgTableModel::data(const QModelIndex& index, int role) const
{
    if (!index.isValid())
        return QVariant();

    const MsgDataTypeDef& myMsg = msgpool.at(index.row());
    bool timeout = myMsg.state == LIN_TIMEOUT_ERROR ? true : false;
    if (role == Qt::DisplayRole)
    {
        switch (index.column())
        {
        case 0:
        {
            return QString("%1").arg(myMsg.index);
        }
        case 1:
        {
            if (myMsg.utcts > 0)
            {
                char formatted_time[32];
                timeval_to_formatted_string(myMsg.utcts, formatted_time, sizeof(formatted_time));
                return QString::fromUtf8(formatted_time);
            }
            else
            {
                return "N/A";
            }
        }
        case 2:
        {
            double timestamp = myMsg.msg.timestamp * 1E-6f;
            QString formattedTime = QString("%1").arg(timestamp, 0, 'f', 3);
            return formattedTime;
        }
        case 3: return QString((myMsg.msg.header.type & BM_ACK_DATA) ? "TX" : "RX"); break;
        case 4: return QString("%1").arg(myMsg.port); break;
        case 5: return msgId(myMsg.msg); break;
        case 6: return msgType(myMsg); break;
        case 7: return msgLength(myMsg.msg, timeout); break;
        case 8: return msgChecksum(myMsg.msg, timeout); break;
        case 9: return msgResult(myMsg); break;
        case 10: return msgPayload(myMsg.msg, timeout); break;
        default: break;
        }
    }
    return QVariant();
}

void LinMsgTableModel::clearAllMessages()
{
    int n = msgpool.size();
    if (n > 0)
    {
        beginRemoveRows(QModelIndex(), 0, n - 1);
        msgpool.clear();
        endRemoveRows();
        emit dataChanged(QModelIndex(), QModelIndex());
    }
}

int LinMsgTableModel::insertMessage(const MsgDataTypeDef &msg)
{
    MsgDataTypeDef msgData = msg;
    bool allChanged = false;
    if (msgpool.size() > 1000000)
    {
        beginRemoveRows(QModelIndex(), 0, 0);
        msgpool.pop_front();
        endRemoveRows();
        allChanged = true;
    }
    int newItemRow = msgpool.size();
    beginInsertRows(QModelIndex(), newItemRow, newItemRow);
    msgpool.push_back(msgData);
    endInsertRows();

    if (allChanged)
    {
        emit dataChanged(QModelIndex(), QModelIndex());
    }
    else
    {
        emit dataChanged(index(newItemRow, 0), index(newItemRow, columnCount() - 1));
    }

    return newItemRow;
}


