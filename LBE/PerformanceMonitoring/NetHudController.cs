using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRStandardAssets.Utils;
using UnityEngine.UI;

namespace Spaces.LBE.PerformanceMonitoring
{
    public class NetHudController : NetworkBehaviour
    {
        bool m_Showing = false;

        public void ControlFPSDisplay()
        {
            if (isServer)
            {
                RpcControlFPSDisplay(!m_Showing);
                m_Showing = !m_Showing;
            }
        }

        //-------------------------------------

        void showFPSLocal(bool bShow)
        {
            NetworkMonitor NetMonitor = GameObject.FindObjectOfType<NetworkMonitor>();
            if (NetMonitor)
            {
                NetMonitor.show(bShow);
            }

        }

        //-------------------------------------

        [ClientRpc]
        public void RpcControlFPSDisplay(bool bShow)
        {
            showFPSLocal(bShow);
        }

        //-------------------------------------
    }
}
