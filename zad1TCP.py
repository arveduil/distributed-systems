import socket
import sys
import pickle
import marshal
import threading
import datetime

from threading import Thread
import time

LOCALHOST = "127.0.0.1"
MULTICAST_IP = '224.1.1.1'
MULTICAST_PORT = 5007
BUFFER_SIZE = 1024


def sendData(ID, nextIp, nextPort: int, data):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((nextIp, nextPort))
    data_bytes = marshal.dumps(data)
    try:
        print(ID, "sending",data["dataType"]," to ", (nextIp, nextPort))

        s.sendall(data_bytes)
    finally:
        s.close()


def generateInitMessage(newClientIp, newUserPort,newClientNeighbourIp,newClientNeighbourPort):
     initMessage = {
        "dataType": "initRequest",
        "newClientIp": newClientIp,
        "newClientPort": newUserPort,
    }
     return initMessage


def generateInitResponsee(neighbourIp, neighbourPort):
     initResponse = {
        "dataType": "initResponse",
        "oldNeighbourIp": neighbourIp,
        "oldNeighbourPort": neighbourPort
     }
     return initResponse

def generateTokenWithMessage(senderId,receiverId,message):
    token = {
        "dataType": "token",
        "isInUse": 1,
        "receiverId": receiverId,
        "senderId": senderId,
        "message": message,
        "TTL": 10
    }
    return token

def generateTokenEmpty():
    token = {
        "dataType": "token",
        "isInUse": 0,
        "receiverId": "",
        "senderId": "",
        "message": "",
        "TTL": 10
    }
    return token

def sendMulticast(ID):
    message = str(datetime.datetime.now())+ " " + ID+" "

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
    sock.setsockopt(socket.IPPROTO_IP, socket.IP_MULTICAST_TTL, 32)
    sock.sendto(bytes(message, 'utf-8'), (MULTICAST_IP, MULTICAST_PORT))


def start(ID, listeningPort, neighbourIpSocketString, owningToken, protocol, messageCountdown,messageReceiver,message):
    neighbourIp = neighbourIpSocketString.split(':')[0]
    neighbourPort = int(neighbourIpSocketString.split(':')[1])
    myIp = LOCALHOST
    messageToSend = 1
    cannotSendMessage = 0

    neighbourIp, neighbourPort = initialize(ID, listeningPort, myIp, neighbourIp, neighbourPort)
    send_first_token_if_owned(ID, neighbourIp, neighbourPort, owningToken)

    while True:
        server_address, sock = createListeningSocket(listeningPort, myIp)

        time.sleep(1)
        conn, addr = sock.accept()
        data_serialized = conn.recv(BUFFER_SIZE)
        data = marshal.loads(data_serialized)
        print(ID,"received data:", data)

        if data["dataType"] == "initRequest":
            neighbourIp, neighbourPort = handle_initRequest(ID, data, neighbourIp, neighbourPort)

        if data["dataType"] == "token":
            print(ID, " received token ")
            sendMulticast(ID)
            data["TTL"] = data["TTL"] - 1

            messageCountdown = messageCountdown - 1
            data = handle_TTL_of_token(data)
            data = handle_receivment_of_token(ID, data)
            if cannotSendMessage <= 0:
              data,messageToSend, cannotSendMessage = send_message_if_possible_and_needed(ID, data, message, messageCountdown, messageReceiver,messageToSend,cannotSendMessage)
            cannotSendMessage= cannotSendMessage -1
            sendData(ID,neighbourIp,neighbourPort,data)
            conn.close()
        sock.close()


def initialize(ID, listeningPort, myIp, neighbourIp, neighbourPort):
    if neighbourPort != listeningPort:
        server_address, sock = createListeningSocket(listeningPort, myIp)
        print(ID, "init on ", server_address)
        neighbourIp, neighbourPort = do_initialization(ID, listeningPort, myIp, neighbourIp, neighbourPort, sock)
        sock.close()
    return neighbourIp, neighbourPort


def send_first_token_if_owned(ID, neighbourIp, neighbourPort, owningToken):
    if owningToken == 1:
        token = generateTokenEmpty()
        sendData(ID, neighbourIp, neighbourPort, token)


def do_initialization(ID, listeningPort, myIp, neighbourIp, neighbourPort, sock):
    initMessage = generateInitMessage(myIp, listeningPort, neighbourIp, neighbourPort)
    sendData(ID, neighbourIp, neighbourPort, initMessage)
    conn, addr = sock.accept()
    data_serialized = conn.recv(BUFFER_SIZE)
    data = marshal.loads(data_serialized)
    if data["dataType"] == "initResponse":
        neighbourIp = data["oldNeighbourIp"]
        neighbourPort = data["oldNeighbourPort"]
        print(ID, "recived init response, now neighbour is", neighbourIp, neighbourPort)
    return neighbourIp, neighbourPort


def handle_initRequest(ID, data, neighbourIp, neighbourPort):
    response = generateInitResponsee(neighbourIp, neighbourPort)
    neighbourIp = data["newClientIp"]
    neighbourPort = data["newClientPort"]
    print(ID, "recived init message, now neighbour is", neighbourIp, neighbourPort)
    sendData(ID, data["newClientIp"], data["newClientPort"], response)
    return neighbourIp, neighbourPort


def send_message_if_possible_and_needed(ID, data, message, messageCountdown, messageReceiver, messageToSend, cannotSendMessage):
    if data["isInUse"] == 0:
        if messageCountdown < 0 and messageToSend == 1:
            messageToSend = 0
            data = generateTokenWithMessage(ID, messageReceiver, message)
            cannotSendMessage = 1
            print(ID, "IS SENDING MESSAGE TO ", messageReceiver)
    return data, messageToSend,cannotSendMessage


def handle_receivment_of_token(ID, data):
    if data["receiverId"] == ID:
        print(ID, "RECEIVED", data["message"], " from ", data["senderId"])
        data = generateTokenEmpty()
    return data


def handle_TTL_of_token(data):
    if data["TTL"] == 0:
        print("Token TTL is equal to 0")
        data = generateTokenEmpty()
    return data

def createListeningSocket(listeningSocket, myIp):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((myIp, listeningSocket))
    s.listen(5)
    return (myIp, listeningSocket), s

def run_tcp_client(ID,main_port,ip_port,has_token,protocol,ticks_for_message,reciver_id,message):
    Thread(target=start, args=(ID, main_port, ip_port, has_token, protocol, ticks_for_message, reciver_id, message)).start()

if __name__ == "__main__":
    ID = sys.argv[1]
    main_port = int(sys.argv[2])
    ip_port = sys.argv[3]
    has_token = int(sys.argv[4])
    protocol = sys.argv[5]
    ticks_for_message = int(sys.argv[6])
    receiver_id = sys.argv[7]
    message = sys.argv[8]
    start(ID, main_port, ip_port, has_token, protocol, ticks_for_message, receiver_id, message)


# client1 = Thread(target= start, args = ("a", 50000, "127.0.0.1:50000", 0, "UDP",12,"e","PRZYPS")).start()
# time.sleep(2)
# client2 = Thread(target= start, args = ("b", 50001, "127.0.0.1:50000", 0, "UDP",8,"b", "22222")).start()
# time.sleep(3)
# client3 = Thread(target= start, args = ("c", 50002, "127.0.0.1:50001", 0, "UDP",8,"d","3333333",)).start()
# time.sleep(4)
# client4 = Thread(target= start, args = ("d", 50003, "127.0.0.1:50002", 1, "UDP",5,"a","1111")).start()