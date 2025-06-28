import cv2
import mediapipe as mp
import json
import time
import numpy as np
from mediapipe.python.solutions.pose import PoseLandmark
from mediapipe.python.solutions.hands import HandLandmark

class Motion:
    def __init__(self, sock):
        self.mpHolistic = mp.solutions.holistic
        self.holistic = self.mpHolistic.Holistic(static_image_mode=False)
        self.mpDraw = mp.solutions.drawing_utils
        self.camera = cv2.VideoCapture(0)
        self.sock = sock
        self.frameId = 0
        self.mode = "idle"
        self.movement = "forward"
        self.calibrated = False
        self.prevX = self.prevY = self.prevZ = None

    def setMode(self, mode):
        old_mode = self.mode
        self.mode = mode
        print(f"🔁 Mode changed from '{old_mode}' to '{mode}'")
        if mode != "calibrate":
            self.calibrated = False
        else:
            # Reset calibrated flag when entering calibrate mode
            self.calibrated = False
            print("📏 Entering calibration mode - will send calibration data")

    def run(self):
        print("📸 Starting frame loop...")
        MOVE_THRESHOLD = 0.01

        while True:
            # 🔄 Non-blocking mode switch
            try:
                self.sock.sock.settimeout(0.01)
                mode_data = self.sock.receiveData()
                if "mode" in mode_data:
                    self.setMode(mode_data["mode"])
            except Exception:
                pass

            if self.mode == "quit":
                print("🛑 Received quit. Exiting loop.")
                break

            success, frame = self.camera.read()
            if not success:
                frame = np.zeros((480, 640, 3), dtype=np.uint8)
                cv2.putText(frame, "NO CAMERA", (200, 240),
                            cv2.FONT_HERSHEY_SIMPLEX, 2, (0, 0, 255), 3)
                cv2.putText(frame, f"Mode: {self.mode}", (10, 30),
                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                cv2.putText(frame, f"Frame: {self.frameId}", (10, 70),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
                results = None
            else:
                rgbImg = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                results = self.holistic.process(rgbImg)

            frameData = {
                "frameId": self.frameId,
                "timeStamp": time.time()
            }

            if results and results.pose_landmarks:
                # build pose dict
                landmarks = results.pose_landmarks.landmark
                frameData["pose"] = {
                    name.name.lower(): {
                        "x": lm.x, "y": lm.y, "z": lm.z, "visibility": lm.visibility
                    }
                    for name, lm in zip(PoseLandmark, landmarks)
                }

                # Calibration (once)
                if self.mode == "calibrate" and not self.calibrated:
                    keypoints = {
                        "left_shoulder": PoseLandmark.LEFT_SHOULDER,
                        "right_shoulder": PoseLandmark.RIGHT_SHOULDER,
                        "left_hip": PoseLandmark.LEFT_HIP,
                        "right_hip": PoseLandmark.RIGHT_HIP
                    }
                    calibrationData = {
                        key: {
                            "x": landmarks[idx.value].x,
                            "y": landmarks[idx.value].y,
                            "z": landmarks[idx.value].z
                        }
                        for key, idx in keypoints.items()
                    }
                    calibrationData.update({
                        "type": "calibrate",
                        "frameId": self.frameId,
                        "timeStamp": time.time(),
                        "status": "calibrated"
                    })
                    self.sock.sendData(json.dumps(calibrationData))
                    print("✅ Sent calibration data.")
                    self.calibrated = True

                # Tracking movement
                if self.mode == "track":
                    # 1) compute hip midpoint
                    lh = landmarks[PoseLandmark.LEFT_HIP.value]
                    rh = landmarks[PoseLandmark.RIGHT_HIP.value]
                    hipX = (lh.x + rh.x) / 2.0
                    hipY = (lh.y + rh.y) / 2.0
                    hipZ = (lh.z + rh.z) / 2.0

                    # 2) if we have a previous position, compare
                    if self.prevX is not None:
                        dx = hipX - self.prevX
                        dz = hipZ - self.prevZ

                        self.movement = "forward"
                        # only if moved beyond threshold
                        if abs(dx) > MOVE_THRESHOLD or abs(dz) > MOVE_THRESHOLD:
                            # pick dominant axis
                            if abs(dx) > abs(dz):
                                self.movement = "left" if dx < 0 else "right"
                            else:
                                self.movement = "forward" if dz < 0 else "backward"

                        frameData["movement"] = self.movement
                        frameData["type"] = "track"

                        # send it out
                        self.sock.sendData(json.dumps(frameData))

                    # 3) store for next frame
                    self.prevX, self.prevY, self.prevZ = hipX, hipY, hipZ

            # draw & display
            if results:
                self.mpDraw.draw_landmarks(frame, results.pose_landmarks,
                                           self.mpHolistic.POSE_CONNECTIONS)
                self.mpDraw.draw_landmarks(frame, results.left_hand_landmarks,
                                           self.mpHolistic.HAND_CONNECTIONS)
                self.mpDraw.draw_landmarks(frame, results.right_hand_landmarks,
                                           self.mpHolistic.HAND_CONNECTIONS)

            cv2.putText(frame, f"Mode: {self.mode}", (10, 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            cv2.putText(frame, f"Mode: {self.movement}", (10, 70),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            cv2.putText(frame, f"Frame: {self.frameId}", (10, 110),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            cv2.imshow("Pose Viewer", frame)
            self.frameId += 1

            if cv2.waitKey(1) == 27:
                print("⎋ ESC pressed. Exiting.")
                break

    def close(self):
        print("📴 Releasing camera and closing sockets.")
        self.camera.release()
        cv2.destroyAllWindows()
        self.sock.close()
