using UnityEngine;

namespace Unity.FPS.Gameplay
{
    class PoseController
    {
        private Vector3 leftShoulder, rightShoulder, leftCalibratedShoulder, rightCalibratedShoulder;

        public void GetShoulder()
        {
            PoseFrame poseFrame = JsonUtility.FromJson<PoseFrame>(PlayerDataHolder.latestJson);

            rightShoulder = new Vector3(
                poseFrame.pose.right_shoulder.x,
                poseFrame.pose.right_shoulder.y,
                poseFrame.pose.right_shoulder.z
            );

            leftShoulder = new Vector3(
                poseFrame.pose.left_shoulder.x,
                poseFrame.pose.left_shoulder.y,
                poseFrame.pose.left_shoulder.z
            );

            Debug.Log("Left Shoulder: " + leftShoulder);
            Debug.Log("Right Shoulder: " + rightShoulder);
        }

        public void GetCalibratedShoulder()
        {
            PoseFrame calibratedPoseFrame = JsonUtility.FromJson<PoseFrame>(CalibrationDataHolder.CalibrationData);

            rightCalibratedShoulder = new Vector3(
                calibratedPoseFrame.pose.right_shoulder.x,
                calibratedPoseFrame.pose.right_shoulder.y,
                calibratedPoseFrame.pose.right_shoulder.z
            );

            leftCalibratedShoulder = new Vector3(
                calibratedPoseFrame.pose.left_shoulder.x,
                calibratedPoseFrame.pose.left_shoulder.y,
                calibratedPoseFrame.pose.left_shoulder.z
            );

            Debug.Log("Left Calibrated Shoulder: " + leftCalibratedShoulder);
            Debug.Log("Right Calibrated Shoulder: " + rightCalibratedShoulder);
        }

        public Vector3 GetLeftShoulder() => leftShoulder;
        public Vector3 GetRightShoulder() => rightShoulder;
        public Vector3 GetLeftCalibratedShoulder() => leftCalibratedShoulder;
        public Vector3 GetRightCalibratedShoulder() => rightCalibratedShoulder;
    }
}
