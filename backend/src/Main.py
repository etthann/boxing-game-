import json
import gzip
from Socket import Socket
from Motion import Motion

def main():
    sock = Socket("127.0.0.1", 8000, 8001)
    motion = Motion(sock)

    try:
        motion.run()  # Central frame loop + mode management
    finally:
        motion.close()
        sock.close()

if __name__ == "__main__":
    main()
