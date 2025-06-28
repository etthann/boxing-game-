import socket
import gzip
import json
import time
import os

class Socket:
    def __init__(self, ip, portTx, portRx):
        self.udpId = ip
        self.udpPortTx = portTx
        self.udpPortRx = portRx
        self.bufferSize = 4096
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind((self.udpId, self.udpPortRx))
        
        # 🔍 Log socket initialization
        print(f"🚀 Socket initialized:")
        print(f"   📡 Listening on: {ip}:{portRx}")
        print(f"   📤 Sending to: {ip}:{portTx}")
        print(f"   🔧 Buffer size: {self.bufferSize}")
        
        # Create logs directory and write startup info
        if not os.path.exists("logs"):
            os.makedirs("logs")
        
        startup_log = {
            "timestamp": time.time(),
            "readable_time": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime()),
            "event": "socket_initialized",
            "listen_address": f"{ip}:{portRx}",
            "send_address": f"{ip}:{portTx}"
        }
        
        with open("logs/connection.log", "a") as f:
            f.write(json.dumps(startup_log) + "\n")

    def sendData(self, data):
        # 📄 Log sent data to file
        log_entry = {
            "timestamp": time.time(),
            "readable_time": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime()),
            "to_address": f"{self.udpId}:{self.udpPortTx}",
            "data": json.loads(data) if isinstance(data, str) else data
        }
        
        # Create logs directory if it doesn't exist
        if not os.path.exists("logs"):
            os.makedirs("logs")
        
        # Write to log file
        with open("logs/sent_data.json", "a") as f:
            f.write(json.dumps(log_entry) + "\n")
        
        # Send the data
        compressed = gzip.compress(data.encode('utf-8'))
        self.sock.sendto(compressed, (self.udpId, self.udpPortTx))
        print(f"📤 Sent data to {self.udpId}:{self.udpPortTx}")
        print(f"📄 Logged to logs/sent_data.json")

    def receiveData(self):
        try:
            data, address = self.sock.recvfrom(self.bufferSize)
            
            # Try to decompress first (for gzip data)
            try:
                decompressed_data = gzip.decompress(data).decode('utf-8')
                print(f"📦 Received compressed data from {address[0]}:{address[1]}")
            except:
                # If decompression fails, try as plain text (for Unity)
                decompressed_data = data.decode('utf-8')
                print(f"📦 Received uncompressed data from {address[0]}:{address[1]}")
            
            parsed_data = json.loads(decompressed_data)
            
            # 📄 Log received data to file
            log_entry = {
                "timestamp": time.time(),
                "readable_time": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime()),
                "from_address": f"{address[0]}:{address[1]}",
                "data": parsed_data
            }
            
            # Create logs directory if it doesn't exist
            if not os.path.exists("logs"):
                os.makedirs("logs")
            
            # Write to log file
            with open("logs/received_data.json", "a") as f:
                f.write(json.dumps(log_entry) + "\n")
            
            print(f"📡 Received data from {address[0]}:{address[1]}")
            print(f"📄 Logged to logs/received_data.json")
            print(f"📦 Data: {parsed_data}")
            
            return parsed_data
            
        except socket.timeout:
            # This is expected when no data is available
            raise
        except Exception as e:
            print(f"❌ Error receiving data: {e}")
            # Log the error
            error_log = {
                "timestamp": time.time(),
                "readable_time": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime()),
                "event": "receive_error",
                "error": str(e)
            }
            
            if not os.path.exists("logs"):
                os.makedirs("logs")
                
            with open("logs/connection.log", "a") as f:
                f.write(json.dumps(error_log) + "\n")
            
            raise

    def close(self):
        self.sock.close()
