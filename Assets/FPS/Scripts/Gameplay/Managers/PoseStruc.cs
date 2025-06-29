namespace Unity.FPS.Gameplay
{
    [System.Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
        public float visibility;
    }

    [System.Serializable]
    public class Pose
    {
        public Landmark nose;
        public Landmark left_eye_inner;
        public Landmark left_eye;
        public Landmark left_eye_outer;
        public Landmark right_eye_inner;
        public Landmark right_eye;
        public Landmark right_eye_outer;
        public Landmark left_ear;
        public Landmark right_ear;
        public Landmark mouth_left;
        public Landmark mouth_right;
        public Landmark left_shoulder;
        public Landmark right_shoulder;
        public Landmark left_elbow;
        public Landmark right_elbow;
        public Landmark left_wrist;
        public Landmark right_wrist;
        public Landmark left_pinky;
        public Landmark right_pinky;
        public Landmark left_index;
        public Landmark right_index;
        public Landmark left_thumb;
        public Landmark right_thumb;
        public Landmark left_hip;
        public Landmark right_hip;
        public Landmark left_knee;
        public Landmark right_knee;
        public Landmark left_ankle;
        public Landmark right_ankle;
        public Landmark left_heel;
        public Landmark right_heel;
        public Landmark left_foot_index;
        public Landmark right_foot_index;
    }

    [System.Serializable]
    public class PoseFrame
    {
        public int frameId;
        public double timeStamp;
        public Pose pose;
        public string movement;
        public string type;
    }
}
