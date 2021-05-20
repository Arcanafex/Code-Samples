using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Spaces.LBE.PerformanceMonitoring;

namespace Spaces.LBE.PerformanceMonitoring
{
    public class NetworkMonitor : MonoBehaviour
    {
        // NetworkMontoring
        NetworkManager m_NetworkManager;
        BoxFilter m_NetworkRTTData = new BoxFilter(10);

        // Optitrack Monitoring
        // frame monitoring
        OptitrackStreamingClient m_OptitrackStreamingClient;
        BoxFilter m_OptitrackFramePerUpdateData = new BoxFilter(10);
        private int m_OptitrackFramePerSecond = 0;
        // stale or missing rigid body data per frame (against monitored list)
        private int m_StaleRigidBodyDataCount = 0;
        // count of unity updates with no new optitrack frames
        private int m_ZeroOptiFrameInUpdateCount = 0;

        private UnityEngine.UI.Text m_Text;

        // toggles for hud items
        public bool bShowNet = true;
        public bool bShowOptiFramePerUpdate = true;
        public bool bShowOptiFramePerSec = true;
        public bool bShowRenderNoOptiFrame = true;
        public bool bShowNoOptiData = true;
        // threshold values for text color
        public float NetTimeYellow = 40.0f;
        public float NetTimeRed = 70.0f;
        public int OptitrackFramePerUpdateRed = 0;
        public int OptitrackFramePerSecondRed = 150;
        // colors for text
        private string ColorGreen = "#00ff00ff";
        private string ColorYellow = "#ffff00ff";
        private string ColorRed = "#ff0000ff";


        private void Start ()
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>();
            m_OptitrackStreamingClient = FindObjectOfType<OptitrackStreamingClient>();

            m_Text = GetComponent<UnityEngine.UI.Text> ();
            if (m_Text)
            {
                m_Text.supportRichText = true;
                m_Text.enabled = false;
            }

            StartCoroutine(UpdatePerfData());
            StartCoroutine(LogData());
        }


        IEnumerator LogData()
        {
            yield return new WaitForSeconds(60);
            Spaces.LBE.DebugLog.Log("networking", m_NetworkRTTData.GetMaxDataValue().ToString());
            m_NetworkRTTData.ResetMaxDataValue();
        }


        IEnumerator UpdatePerfData()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.3f);

                // Update Data
                UpdateNetworkClientRTT();
                UpdateOptitrackData();

                // Per Requested Hud, draw complete richtext line
                string richTextRep = "";
                if (bShowNet)
                {
                    // Unity Network Client RTT in millisecs
                    float aveClientRTT = m_NetworkRTTData.GetFilteredData();
                    float maxClientRTT = m_NetworkRTTData.GetMaxDataValue();
                    richTextRep = "<color=";
                    richTextRep += (aveClientRTT > NetTimeYellow) ? (aveClientRTT > NetTimeRed) ? ColorRed : ColorYellow : ColorGreen;
                    richTextRep += ">" + string.Format("{0:F1}", aveClientRTT) + "</color>  <color=";
                    richTextRep += (maxClientRTT > NetTimeYellow) ? (maxClientRTT > NetTimeRed) ? ColorRed : ColorYellow : ColorGreen;
                    richTextRep += ">" + string.Format("{0:F1}", maxClientRTT) + "</color> Net ms    \n";
                }
                if (bShowOptiFramePerUpdate)
                {
                    // Optitrack Frames Received for this Unity Update/Render
                    float optitrackFramesPerUpdate = m_OptitrackFramePerUpdateData.GetFilteredData();

                    richTextRep += "<color=";
                    richTextRep += (optitrackFramesPerUpdate > OptitrackFramePerUpdateRed) ? ColorGreen : ColorRed;
                    richTextRep += ">" + string.Format("{0:F1}", optitrackFramesPerUpdate) + "</color> Opt FPU   \n";
                }
                if (bShowOptiFramePerSec)
                {
                    // Optitrack Frames per Sec (sent from Motive at 180/sec)
                    richTextRep += "<color=";
                    richTextRep += (m_OptitrackFramePerSecond > OptitrackFramePerSecondRed) ? ColorGreen : ColorRed;
                    richTextRep += ">" + m_OptitrackFramePerSecond + "</color> Opt FPS   \n";
                }
                if (bShowRenderNoOptiFrame)
                {
                    // Number of Unity Update/Renders that did not receive any
                    // optitrack data since last Unity Update/Render
                    richTextRep += "<color=";
                    richTextRep += (m_ZeroOptiFrameInUpdateCount <= 0) ? ColorGreen : ColorYellow;
                    richTextRep += ">" + m_ZeroOptiFrameInUpdateCount + "</color> Opt NoFrm\n";
                }
                if (bShowNoOptiData)
                {
                    // Number of Optitrack frames received that contained
                    // either no data or exactly the same data
                    // (i.e. Stale Frame Data from Optitrack)

                    richTextRep += "<color=";
                    richTextRep += (m_StaleRigidBodyDataCount <= 0) ? ColorGreen : ColorYellow;
                    richTextRep += ">" + m_StaleRigidBodyDataCount + "</color> Opt Stale \n";
                }

                if (m_Text)
                {
                    m_Text.text = richTextRep;
                }
            }
        }


        public void Toggle()
        {
            m_Text.enabled = GlobalSettings.m_DevelopmentBuild && !m_Text.enabled;
        }


        public void show(bool bShow)
        {
            m_Text.enabled = GlobalSettings.m_DevelopmentBuild && bShow;
        }


        private void UpdateNetworkClientRTT()
        {
            // get current RTT from Unity Network Client
            if (m_NetworkManager != null)
            {
                if (m_NetworkManager.client != null /*will be null on server*/)
                {
                    try
                    {
                        float rtt = (float)m_NetworkManager.client.GetRTT();
                        //DBGSpaces.LBE.DebugLog.Log("networking", "rtt value: " + rtt);
                        m_NetworkRTTData.AddDataPoint(rtt);
                    }
                    catch (System.Exception e)
                    {
                        Spaces.LBE.DebugLog.Log("networking", "Error Querying client RTT in NetworkMonitor: " + e.Message);
                    }
                }
            }
        }


        private void UpdateOptitrackData()
        {
            if (m_OptitrackStreamingClient)
            {
                try
                {
                    m_OptitrackFramePerUpdateData.AddDataPoint(m_OptitrackStreamingClient.FramesPerUpdate());
                    m_OptitrackFramePerSecond = m_OptitrackStreamingClient.FramesPerSecond();
                    m_StaleRigidBodyDataCount = m_OptitrackStreamingClient.StaleRigidBodyInFrameCount();
                    m_ZeroOptiFrameInUpdateCount = m_OptitrackStreamingClient.ZeroFrameUpdateCount();
                }
                catch (System.Exception e)
                {
                    Spaces.LBE.DebugLog.Log("networking", "Error Querying Optitrack Stream Client for Time Between Frames: " + e.Message);
                }
            }
        }


    }
}
