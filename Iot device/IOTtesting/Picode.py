import requests
import asyncio
import cv2
import numpy as np
import fcntl
import struct
from datetime import datetime
import websockets
import json
import uuid
import socket
from ultralytics import YOLO
from picamera2 import Picamera2

# Device info

DEVICE_ID = "cam01" 
DEVICE_NAME="Pi Camera IOT lab"
DEVICE_LOCATION="10.869800, 106.802771"


# Configuration

WS_SERVER_URI = f"ws://192.168.63.157:5000/ws/upload/{DEVICE_ID}"
CONTROL_SERVER_URI = f"ws://192.168.63.157:5000/ws/control/{DEVICE_ID}"
SERVER_STATUS_URI = f"http://192.168.63.157:5000/api/info/{DEVICE_ID}"
TARGET_WIDTH = 1000
TARGET_HEIGHT =700
JPEG_QUALITY = 40
TARGET_FPS = 35




# Initialize the Pi Camera
picam2 = Picamera2()
picam2.preview_configuration.main.size = (TARGET_WIDTH, TARGET_HEIGHT)
picam2.preview_configuration.main.format = "RGB888"
picam2.configure("preview")
picam2.start()
print("Camera initialized")

streaming_task = None  
async def control_loop():
    global streaming_task

    while True:
        try:
            async with websockets.connect(CONTROL_SERVER_URI) as websocket:
                print("Connected to control server")




                # Announce online status
                await websocket.send(json.dumps({"status": "online"}))

                while True:
                    message = await websocket.recv()
                    print(f"Received: {message}")
                    try:
                        command = json.loads(message)
                        cmd = command.get("command")
                        print(f"Received command: {cmd}")

                        if cmd == "start_stream":
                            if streaming_task is None or streaming_task.done():
                                print("Starting camera stream...")
                                streaming_task = asyncio.create_task(send_video_stream())
                            else:
                                print("Stream already running.")

                        # if streaming_task is None or streaming_task.done():
                        #     print("Starting camera stream...")

                        #     try:
                        #         picam2.preview_configuration.main.size = (TARGET_WIDTH, TARGET_HEIGHT)
                        #         picam2.preview_configuration.main.format = "RGB888"
                        #         picam2.configure("preview")
                        #         picam2.start()
                        #         print("Camera started for streaming.")
                        #     except Exception as start_err:
                        #         print(f"Error starting camera: {start_err}")

                            # streaming_task = asyncio.create_task(send_video_stream())


                        elif cmd == "stop_stream":
                            if streaming_task and not streaming_task.done():
                                print("Stopping camera stream...")
                                streaming_task.cancel()
                                try:
                                    await streaming_task  # Gracefully finish the task
                                except asyncio.CancelledError:
                                    print("Streaming task cancelled cleanly.")
                                except Exception as err:
                                    print(f"Error while cancelling streaming task: {err}")
        
                                # Stop the camera if needed
                                try:
                                    picam2.stop()
                                    print("Camera stopped.")
                                except Exception as cam_err:
                                    print(f"Error stopping camera: {cam_err}")
                            else:
                                print("Stream is not running.")

                        elif cmd == "update_settings":
                            params = command.get("parameter", {})
                            print(f"Updating device settings: {params}")

                            # Get parameters from command (fallbacks if missing)
                            deviceName = params.get("deviceName", DEVICE_NAME)
                            location = params.get("location", DEVICE_LOCATION)
                            width = params.get("width", TARGET_WIDTH)
                            height = params.get("height", TARGET_HEIGHT)
                            quality = params.get("quality", JPEG_QUALITY)
                            fps = params.get("fps", TARGET_FPS)

                            # Apply changes to global variables
                            DEVICE_NAME = deviceName
                            DEVICE_LOCATION = location
                            TARGET_WIDTH = width
                            TARGET_HEIGHT = height
                            JPEG_QUALITY = quality
                            TARGET_FPS = fps

                            # === Apply camera reconfiguration ===
                            try:
                                # Stop the camera before reconfiguring
                                picam2.stop()
        
                                # Update camera preview settings
                                picam2.preview_configuration.main.size = (TARGET_WIDTH, TARGET_HEIGHT)
                                picam2.preview_configuration.main.format = "RGB888"
                                picam2.configure("preview")

                                # Restart camera
                                picam2.start()
                                print("Camera reconfigured successfully.")
                            except Exception as cam_err:
                                print(f"Failed to apply camera settings: {cam_err}")

                            print(f"Applied settings - Name: {DEVICE_NAME}, Location: {DEVICE_LOCATION}, Width: {width}, Height: {height}, Quality: {quality}, FPS: {fps}")


                        await websocket.send(json.dumps({"status": "ack", "command": cmd}))
                    except Exception as e:
                        print(f"Error parsing command: {e}")

        except Exception as e:
            print(f"Control connection error: {e}. Retrying in 5 seconds...")
            await asyncio.sleep(5)


async def send_video_stream():
    print(f"Connecting to WebSocket server at {WS_SERVER_URI}...")
    try:
        async with websockets.connect(WS_SERVER_URI) as ws:
            print("Connected to the streaming WebSocket server.")

            last_sent_time = datetime.min
            frame_interval_ms = 1000 / TARGET_FPS

            while True:
                now = datetime.now()
                elapsed_ms = (now - last_sent_time).total_seconds() * 1000
                if elapsed_ms < frame_interval_ms:
                    await asyncio.sleep((frame_interval_ms - elapsed_ms) / 1000)
                    continue

                last_sent_time = now

                # Capture frame from Pi Camera
                frame = await asyncio.to_thread(picam2.capture_array)

                if frame is None or frame.shape[0] == 0:
                    print("Invalid frame received, skipping...")
                    continue
		

                # === Resize and encode frame ===
                
                encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]
                result, encoded_image = cv2.imencode('.jpg', frame, encode_param)

                if result:
                    await ws.send(encoded_image.tobytes())

    except asyncio.CancelledError:
        print("Camera stream task cancelled.")
    except Exception as ex:
        print(f"Error: {ex}")

def get_ip(interface='wlan0'):
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        iface_bytes = struct.pack('256s', interface[:15].encode('utf-8'))
        ip_bytes = fcntl.ioctl(s.fileno(), 0x8915, iface_bytes)[20:24]
        return socket.inet_ntoa(ip_bytes)
    except Exception as e:
        print(f"Failed to get IP for interface {interface}: {e}")
        return "0.0.0.0"


def get_mac(interface='wlan0'):
    try:
        with open(f'/sys/class/net/{interface}/address') as f:
            return f.read().strip()
    except:
        return "00:00:00:00:00:00"

def get_time():
    current_time = datetime.now()
    return current_time.strftime('%Y-%m-%d %H:%M:%S')

def get_location():
   location = DEVICE_LOCATION # use gps reader instead
   return location


def post_status(status):
    payload = {
        "deviceId": DEVICE_ID,
        "deviceName": DEVICE_NAME,
        "location": get_location(),
        "ipAddress": get_ip(),
        "macAddress": get_mac(),
        "status": status,
        "height": TARGET_HEIGHT,
        "width":  TARGET_WIDTH,
        "quality" : JPEG_QUALITY,
        "fps"   : TARGET_FPS,
        "lastSeen" : get_time()
    }

    headers = {'Content-Type': 'application/json'}
    try:
        response = requests.post(SERVER_STATUS_URI, data=json.dumps(payload), headers=headers)
        print(f"Status posted. Server responded with: {response.status_code}")
    except Exception as e:
        print(f"Error posting status: {e}")

# Run the control loop
if __name__ == "__main__":
    post_status(status=True)
    asyncio.run(control_loop())

