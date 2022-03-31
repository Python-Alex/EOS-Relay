import time
import json
import socket
import threading

class Client1:
    clientId = "72db8012-1f16-4ec7-b993-9153f8621cef"
    username = "my_new_name"
    password = "asdfadsf"
    email = "obfuscate@riseup.net"

class Client2:
    clientId = "92c85e4d-6413-42ad-9fe3-4516a61a325f"
    username = "also_new_name"
    password = "asdasd"
    email = "obfuscate@gmail.com"


def Client1Process():
    client = socket.socket()
    client.connect(('127.0.0.1', 52030))

    SendPayload(client, CreatePayload(14, f"{Client1.username};{Client1.password}", Client1.clientId))
    time.sleep(0.25)
    SendPayload(client, CreatePayload(1, "You're a bitch", Client2.clientId))

def CreatePayload(flag: int, content: str, clientid: str):
    return json.dumps({"flag": flag, "content": content, "clientId": clientid}).encode()

def SendPayload(socket: socket.socket, payload: bytes):
    socket.send(payload)

threading.Thread(target=Client1Process).start()
while(True):
    time.sleep(1)
#threading.Thread(target=Client2Process).start()