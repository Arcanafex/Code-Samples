using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPC.Tools
{
    public class CameraControl : MonoBehaviour
    {
        public enum MatchCamera
        {
            ViewPlayer = -1,
            ViewTV = 0,
            ViewTop,
            View34,
            ViewStats,

            ViewServe,
            ViewReturn,

            ViewSetRecap,
            ViewMatchRecap
        }

        public enum CameraMode
        {
            Player,
            Match
        }

        public Transform Pivot;
        public Transform CraneArm;

        public float OrbitSpeed = 1;
        public float CraneSpeed = 1;

        public CameraMode ViewMode;

        public Camera StadiumCamera;
        public SetCameraSignal CameraControlSignal;

        public float Scale { get; set; }

        private float m_baseFPS = 60;

        private void Start()
        {
            SetCameraView(CameraControlSignal);
        }

        private void Update()
        {
            if (StadiumCamera)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    Pivot.Rotate(Vector3.up, OrbitSpeed * (m_baseFPS * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    Pivot.Rotate(Vector3.down, OrbitSpeed * (m_baseFPS * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    CraneArm.Rotate(Vector3.left, CraneSpeed * (m_baseFPS * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    CraneArm.Rotate(Vector3.right, CraneSpeed * (m_baseFPS * Time.deltaTime));
                }
            }
        }

        public void UpdateView(MatchCamera view)
        {
            CameraControlSignal.CameraView = view;
            SetCameraView(CameraControlSignal);
        }

        public void SetViewPlayer(bool view)
        {
            if (view)
                UpdateView(MatchCamera.ViewPlayer);
        }

        public void SetViewTV(bool view)
        {
            if (view)
                UpdateView(MatchCamera.ViewTV);
        }

        public void SetViewTop(bool view)
        {
            if (view)
                UpdateView(MatchCamera.ViewTop);
        }

        public void SetView34(bool view)
        {
            if (view)
                UpdateView(MatchCamera.View34);
        }

        /// <summary>
        /// Update the camera settings according to the requested camera view
        /// </summary>
        /// <param name="camera"></param>
        public void SetCameraView(SetCameraSignal signal)
        {
            StadiumCamera.focalLength = 7.89374f;
            StadiumCamera.sensorSize = new Vector2(4.8f, 3.5f);

            switch (signal.CameraView)
            {
                case MatchCamera.ViewPlayer:
                    {
                        StadiumCamera.transform.SetParent(CraneArm);
                        StadiumCamera.transform.position = new Vector3(0, 1.25f, 3);
                        StadiumCamera.transform.eulerAngles = new Vector3(0, 180, 0);
                        StadiumCamera.focalLength = 14f;
                        StadiumCamera.sensorSize = new Vector2(12.52f, 7.41f);
                        signal.FOV = 60f;

                        break;
                    }
                case MatchCamera.ViewTV:
                    {
                        // old version :
                        //StadiumCamera.transform.position = signal.OpponentSide ? new UnityEngine.Vector3(0, 13, 40) : new UnityEngine.Vector3(0, 13, -40);
                        //StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new UnityEngine.Vector3(18, 180, 0) : new UnityEngine.Vector3(18, 0, 0);

                        float x = 0;
                        float y = 6.9f;
                        float z = 27.9f;

                        StadiumCamera.transform.position = signal.OpponentSide ? new Vector3(x, y, z) : new Vector3(x, y, -z);
                        StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new Vector3(18, 180, 0) : new Vector3(18, 0, 0);
                        StadiumCamera.focalLength = 14f;
                        StadiumCamera.sensorSize = new Vector2(12.52f, 7.41f);
                        signal.FOV = 29.6462f;
                        break;
                    }
                case MatchCamera.ViewTop:
                    {
                        // No opponent side on Top view
                        StadiumCamera.transform.position = new Vector3(0, 60, 0);
                        StadiumCamera.transform.eulerAngles = new Vector3(90, -90, 0);
                        // StadiumCamera.transform.position = signal.OpponentSide ? new UnityEngine.Vector3(0, 60, 0) : new UnityEngine.Vector3(0, 60, 0);
                        // StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new UnityEngine.Vector3(90, 90, 0) : new UnityEngine.Vector3(90, -90, 0);
                        break;
                    }
                case MatchCamera.View34:
                    {
                        float x = 12;
                        float y = Scale > 2 ? 13 : 10;
                        float z = Scale > 2 ? 38 : 30;

                        StadiumCamera.transform.position = signal.OpponentSide ? new Vector3(x, y, z) : new Vector3(-x, y, -z);
                        StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new Vector3(18, 200, 0) : new Vector3(18, 20, 0);
                        break;
                    }
                case MatchCamera.ViewStats:
                    {
                        // No opponent side on Stats view
                        StadiumCamera.transform.position = new Vector3(0, 2.5f, -21);
                        StadiumCamera.transform.eulerAngles = new Vector3(0, 0, 0);
                        // StadiumCamera.transform.position = signal.OpponentSide ? new UnityEngine.Vector3(0, 2.5f, 21) : new UnityEngine.Vector3(0, 2.5f, -21);
                        // StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new UnityEngine.Vector3(0, 180, 0) : new UnityEngine.Vector3(0, 0, 0);
                        break;
                    }
                case MatchCamera.ViewServe:
                    {
                        StadiumCamera.transform.position = signal.OpponentSide ? new Vector3(0, 19, 27) : new Vector3(0, 19, -27);
                        StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new Vector3(40, 180, 0) : new Vector3(40, 0, 0);
                        break;
                    }
                case MatchCamera.ViewReturn:
                    {
                        float x = 0;
                        float y = 17;
                        float z = Scale > 2 ? 35 : 30;

                        StadiumCamera.transform.position = signal.OpponentSide ? new Vector3(x, y, z) : new Vector3(x, y, -z);
                        StadiumCamera.transform.eulerAngles = signal.OpponentSide ? new Vector3(30, 180, 0) : new Vector3(30, 0, 0);
                        break;
                    }
                case MatchCamera.ViewSetRecap:
                    {
                        //StadiumCamera.transform.position = new UnityEngine.Vector3(0, 3, -2);
                        //StadiumCamera.transform.eulerAngles = new UnityEngine.Vector3(20, -90, 0);
                        StadiumCamera.transform.position = new Vector3(11, 1.4f, -5.7f);
                        StadiumCamera.transform.eulerAngles = new Vector3(-10, -90, 0);
                        signal.FOV = 59f;
                        break;
                    }
                case MatchCamera.ViewMatchRecap:
                    {
                        StadiumCamera.transform.position = new Vector3(15, 15, Scale > 2 ? -34 : -30);
                        StadiumCamera.transform.eulerAngles = new Vector3(22, -25, 0);
                        break;
                    }
            }

            StadiumCamera.fieldOfView = signal.FOV - signal.Offset;
        }

        [System.Serializable]
        public class SetCameraSignal
        {
            public MatchCamera CameraView;
            public float FOV;
            public float Offset;
            public bool OpponentSide;
        }
    }
}
