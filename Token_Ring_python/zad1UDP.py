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
MULTICAST_PORT_1 = 5007
MULTICAST_PORT_2 = 5008


def sendData(ID, nextIp, nextPort: int, data):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = (nextIp, nextPort)
    try:
        print(ID, "sending",data["dataType"]," to ", server_address)
        data_bytes = marshal.dumps(data)
        sent = sock.sendto(data_bytes, server_address)
    finally:
        sock.close()

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
    sock.sendto(bytes(message, 'utf-8'), (MULTICAST_IP, MULTICAST_PORT_1))
    sock.sendto(bytes(message, 'utf-8'), (MULTICAST_IP, MULTICAST_PORT_2))


def start(ID, listeningPort, neighbourIpSocketString, owningToken, protocol, messageCountdown,messageReceiver,message):
    neighbourIp = neighbourIpSocketString.split(':')[0]
    neighbourPort = int(neighbourIpSocketString.split(':')[1])
    myIp = LOCALHOST
    messageToSend = 1
    cannotSendMessage = 0

    server_address, sock = createListeningSocket(listeningPort, myIp)
    print(ID,"started listening on ", server_address)

    if neighbourPort != listeningPort:
        neighbourIp, neighbourPort = do_initialization(ID, listeningPort, myIp, neighbourIp, neighbourPort, sock)
        send_first_token_if_owned(ID, neighbourIp, neighbourPort, owningToken)


    while True:
        data_serialized, address = sock.recvfrom(4096)
        time.sleep(1)
        data = marshal.loads(data_serialized)
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
                data, messageToSend, cannotSendMessage = send_message_if_possible_and_needed(ID, data, message, messageCountdown, messageReceiver, messageToSend,cannotSendMessage)

            cannotSendMessage = cannotSendMessage - 1
            sendData(ID,neighbourIp,neighbourPort,data)


def send_first_token_if_owned(ID, neighbourIp, neighbourPort, owningToken):
    if owningToken == 1:
        token = generateTokenEmpty()
        sendData(ID, neighbourIp, neighbourPort, token)


def do_initialization(ID, listeningPort, myIp, neighbourIp, neighbourPort, sock):
    initMessage = generateInitMessage(myIp, listeningPort, neighbourIp, neighbourPort)
    sendData(ID, neighbourIp, neighbourPort, initMessage)
    data_serialized, address = sock.recvfrom(4096)
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
    if data["isInUse"] == 0 and cannotSendMessage == 0:
        if messageCountdown < 0 and messageToSend == 1:
            messageToSend = 0
            data = generateTokenWithMessage(ID, messageReceiver, message)
            cannotSendMessage = 2
            print(ID, "IS SENDING MESSAGE TO ", messageReceiver)
    return data, messageToSend, cannotSendMessage


def handle_receivment_of_token(ID, data):
    if data["receiverId"] == ID:
        print(ID, "received", data["message"], " from ", data["senderId"])
        data = generateTokenEmpty()
    return data


def handle_TTL_of_token(data):
    if data["TTL"] == 0:
        print("Token TTL is equal to 0")
        data = generateTokenEmpty()
    return data


def createListeningSocket(listeningSocket, myIp):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = (myIp, listeningSocket)
    sock.bind(server_address)
    return server_address, sock


def run_udp_client(ID,main_port,ip_port,has_token,protocol,ticks_for_message,reciver_id,message):
    Thread(target=start, args=(ID, main_port, ip_port, has_token, protocol, ticks_for_message, reciver_id, message,)).start()

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
