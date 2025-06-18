





from ast import Name
import asyncio
import cv2
from datetime import datetime
import websockets
import socket
import json
import requests
import uuid
import datetime
import time
from ultralytics import YOLO

# Device info

DEVICE_ID = "cam01" # You can use your actual device id or MAC
DEVICE_NAME="Pi Camera IOT lab"
DEVICE_LOCATION="10.869800, 106.802771"

# Configuration
WS_SERVER_URI = f"ws://iowebapp.tryasp.net/ws/upload/{DEVICE_ID}"
CONTROL_SERVER_URI = f"ws://iowebapp.tryasp.net/ws/control/{DEVICE_ID}"
SERVER_STATUS_URI = f"http://iowebapp.tryasp.net/api/info/{DEVICE_ID}"

# Configuration(for testing)
WS_SERVER_URI = f"ws://localhost:5000/ws/upload/{DEVICE_ID}"
CONTROL_SERVER_URI = f"ws://localhost:5000/ws/control/{DEVICE_ID}"
SERVER_STATUS_URI = f"http://localhost:5000/api/info/{DEVICE_ID}"


# Immage quality setting
TARGET_WIDTH = 1280
TARGET_HEIGHT = 1024
JPEG_QUALITY = 35
TARGET_FPS = 40

# ==== Initialize YOLO Model ====
model_path = "yolov8n(old).pt"  # Or your fire detection model path
model = YOLO(model_path)

print(" Fire detection model loaded")

streaming_task = None  # Global streaming task tracker


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

                        elif cmd == "stop_stream":
                            if streaming_task and not streaming_task.done():
                                print("Stopping camera stream...")
                                streaming_task.cancel()
                                await asyncio.sleep(1)  # Let it cleanup
                            else:
                                print("Stream is not running.")

                        elif cmd == "update_settings":
                            params = command.get("parameter", {})
                            print(f"Updating device settings: {params}")

                            # Here you would apply the settings as needed
                            deviceName = params.get("deviceName", "Pi Camera IOT lab")
                            location = params.get("location","10.869800, 106.802771" )
                            height = params.get("height", 480)
                            width = params.get("width", 640)
                            quality = params.get("quality", 35)
                            fps = params.get("fps", 35)

                            # Optional: store these in global variables or apply to camera config
                            # e.g. camera.set_resolution(width, height)
                            
                            DEVICE_NAME=deviceName
                            DEVICE_LOCATION=location
                            TARGET_WIDTH = height
                            TARGET_HEIGHT = width
                            JPEG_QUALITY = quality
                            TARGET_FPS = fps

                            print(f"Applied settings -Name: {DEVICE_NAME},Location: {DEVICE_LOCATION} ,Width: {width}, Height: {height}, Quality: {quality}, FPS: {fps}")
                        

                        await websocket.send(json.dumps({"status": "ack", "command": cmd}))
                    except Exception as e:
                        print(f"Error parsing command: {e}")

        except Exception as e:
            print(f"Control connection error: {e}. Retrying in 5 seconds...")
            await asyncio.sleep(5)

async def send_video_stream():
    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("Cannot open camera.")
        return

    print(f"Connecting to WebSocket server at {WS_SERVER_URI}...")
    try:
        async with websockets.connect(WS_SERVER_URI) as ws:
            print("Connected to the streaming WebSocket server.")

            frame_interval = 1.0 / TARGET_FPS
            last_sent_time = time.perf_counter()

            while True:
                ret, frame = cap.read()
                if not ret:
                    print("Failed to grab frame.")
                    break

                now = time.perf_counter()
                elapsed = now - last_sent_time
                if elapsed < frame_interval:
                    await asyncio.sleep(frame_interval - elapsed)
                    continue
                last_sent_time = now
            
          
                # === Resize and encode frame ===
                resized_frame = cv2.resize(frame, (TARGET_WIDTH, TARGET_HEIGHT), interpolation=cv2.INTER_AREA)
                encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]
                result, encoded_image = cv2.imencode('.jpg', resized_frame, encode_param)
                
                jpeg_bytes = encoded_image.tobytes()
                if result:
                    await ws.send(encoded_image.tobytes())
                  
    except asyncio.CancelledError:
        print("Camera stream task cancelled.")
    except Exception as ex:
        print(f"Error: {ex}")
    finally:
        cap.release()
        print("Camera released.")
    









def get_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(('8.8.8.8', 80))
        return s.getsockname()[0]
    except:
        return "0.0.0.0"
    finally:
        s.close()

def get_mac():
    mac = ':'.join(['{:02x}'.format((uuid.getnode() >> i) & 0xff) for i in range(0,8*6,8)][::-1])
    return mac

def get_time():
    current_time = datetime.datetime.now()
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

post_status(status=True)




# Run the control loop
if __name__ == "__main__":
    asyncio.run(control_loop())
    
    # # Load your trained YOLOv8n PyTorch model
    # model = YOLO("yolov8n(old).pt")

    # # Export to ONNX format
    # model.export(format="onnx", opset=12)  # opset 12 is common and compatible

    # print("export done")

    










