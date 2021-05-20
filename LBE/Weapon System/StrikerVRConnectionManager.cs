using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

namespace Spaces.LBE
{
    public class StrikerVRConnectionManager : MonoBehaviour
    {
        public string portName = "UNASSIGNED";
        private int baudRate = 57600;

        public SerialPort port { get; private set; }
        public XBee radio { get; private set; }
        public static StrikerVRConnectionManager Instance { get; private set; }

        public delegate void InputReceived(System.Int16 address, byte[] message);
        public event InputReceived OnInputReceived;

        protected virtual void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                this.gameObject.SetActive(false);
                return;
            }

            radio = new XBee();

            if (Spaces.LBE.MachineConfigurationManager.instance != null && portName == "UNASSIGNED")
            {
                string strikerConfigPort = Spaces.LBE.MachineConfigurationManager.instance.GetStrikerCOMPort();

                if (!string.IsNullOrEmpty(strikerConfigPort))
                {
                    portName = strikerConfigPort;
                }
            }

            port = new SerialPort(portName, baudRate)
            {
                ReadTimeout = 2,
                WriteTimeout = 2
            };

            DebugLog.Log("inputoutput", "StrikerVRConnectionManager.Awake [Configuring Port] port: " + portName + ", rate: " + baudRate);
        }

        protected virtual void OnEnable()
        {
            DebugLog.Log("inputoutput", "StrikerVRConnectionManager.OnEnable [Opening Port] port: " + portName + ", rate: " + baudRate);
            port.Open();
            radio.rx16PacketReceived += RXReceived;
        }


        protected virtual void OnDisable()
        {
            if (port != null && port.IsOpen)
            {
                DebugLog.Log("inputoutput", "StrikerVRConnectionManager.OnDisable [Closing Port] port: " + portName + ", rate: " + baudRate);
                port.Close();
            }

            radio.rx16PacketReceived -= RXReceived;
        }


        protected virtual void Update()
        {
            if (radio != null)
            {
                Read();
            }
        }


        protected virtual void Read()
        {
            List<byte> data = new List<byte>();

            try
            {
                while (true)
                {
                    int b = port.ReadByte();
                    if (b >= 0)
                    {
                        data.Add((byte)b);
                    }
                    else
                    {
                        break;
                    }
                }

            }
            catch
            {
                //thanks Mono
            }

            if (data.Count > 0)
            {
                radio.ParseStream(data.ToArray());
            }
        }

        protected virtual void RXReceived(XBee.RXPacket p)
        {
            var addr16 = p.address.addr16;
            System.Array.Reverse(addr16);
            System.Int16 address = System.BitConverter.ToInt16(addr16, 0);

            if (OnInputReceived != null)
                OnInputReceived(address, p.data);
        }

        public virtual void Send(System.Int16 address, byte[] message)
        {
            //TODO: set up a dictionary of XBee.Address refs so we don't have to keep creating them
            var strikerAddress = new XBee.Address();
            var addressBytes = System.BitConverter.GetBytes(address);
            System.Array.Reverse(addressBytes);
            strikerAddress.addr16 = addressBytes;


            var payload = XBee.TXRequest16(strikerAddress, 1, message);
            port.Write(payload, 0, payload.Length);
        }

        public void DebugReceivePacket(XBee.RXPacket p)
        {
            RXReceived(p);
        }
    }
}
