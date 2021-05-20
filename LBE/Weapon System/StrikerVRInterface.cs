using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine.UI;

namespace Spaces.LBE
{
    public class StrikerVRInterface : MonoBehaviour
    {
        public enum WeaponMode { RIFLE = 10, CHAINSAW = 20, BLASTER = 30 }

        public enum RifleAction { ENABLE, PING, JAM, SINGLE, BURST, RELOAD, TRIGGER_UP }
        public enum RifleMode { SINGLE, BURST, AUTO }

        public enum ChainsawAction { ENABLE, IDLE, ACTIVE }
        public enum BlasterAction { ENABLE, IDLE, CHARGE, SINGLE }

        public enum Button { Trigger = 0, ModeRight, ModeLeft, Reload }

        public enum Command { SETMODE = 0xFF, SETAMMO = 0xFE }

        protected const int MAXAMMO = 254;

        [Header("Identification")]
        public int address;

        public GunSystem gun;

        [Header("Buttons")]
        public Material normalMaterial;
        public Material pressedMaterial;
        public GameObject[] Buttons;
        public bool[] ButtonValues = new bool[4];

        protected WeaponMode weapon;
        protected int action;
        protected int mode;
        protected int shotsRemaining;

        protected virtual void OnEnable()
        {
            if (StrikerVRConnectionManager.Instance)
            {
                StrikerVRConnectionManager.Instance.OnInputReceived += InputReceived;
            }
        }

        protected virtual void OnDisable()
        {
            if (StrikerVRConnectionManager.Instance)
            {
                StrikerVRConnectionManager.Instance.OnInputReceived -= InputReceived;
            }
        }

        private void Update()
        {
            if (weapon == WeaponMode.RIFLE && gun.TriggerDown)
            {
                if ((RifleAction)action == RifleAction.TRIGGER_UP)
                    gun.ToggleTrigger(false);
            }
        }

        public void initialize(int nAddress)
        {
            address = nAddress;
        }

        protected virtual void InputReceived(System.Int16 address, byte[] payload)
        {
            if (this.address != address)
            {
                return;
            }

            weapon = (WeaponMode)payload[0];

            byte buttonFlags = payload[1];
            ButtonValues[0] = (buttonFlags & 0x01) > 0;
            ButtonValues[1] = (buttonFlags & 0x02) > 0;
            ButtonValues[2] = (buttonFlags & 0x04) > 0;
            ButtonValues[3] = (buttonFlags & 0x08) > 0;

            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].GetComponent<MeshRenderer>().sharedMaterial = ButtonValues[i] ? pressedMaterial : normalMaterial;
            }

            action = payload[2];
            mode = payload[3];
            shotsRemaining = payload[4];

            switch (weapon)
            {
                case WeaponMode.RIFLE:
                    {
                        var rifleAction = (RifleAction)action;
                        var rifleMode = (RifleMode)mode;

                        if (gun)
                        {

                            if ((RifleAction)action == RifleAction.TRIGGER_UP)
                            {
                                gun.ToggleTrigger(false);
                            }
                            else if ((RifleAction)action == RifleAction.SINGLE)
                            {
                                if (rifleMode == RifleMode.SINGLE)
                                {
                                    gun.Fire();
                                }
                                else
                                {
                                    gun.ToggleTrigger(true);
                                }
                            }
                            else if ((RifleAction)action == RifleAction.RELOAD)
                            {
                                gun.Reload();
                                SetAmmo(gun.shotsRemaining);
                            }

                            // infinite ammo
                            if (gun.MagazineSize < 0 && shotsRemaining < 10)
                            {
                                SetAmmo(MAXAMMO);
                            }
                        }
                        break;
                    }
                case WeaponMode.CHAINSAW:
                    {
                        Debug.Log("I'm A Chainsaw!!!");
                        var chainsawAction = (ChainsawAction)action;
                        var chainsawPower = mode;
                    }
                    break;
                case WeaponMode.BLASTER:
                    {
                        Debug.Log("I'm A Blaster!");
                        var blasterAction = (BlasterAction)action;
                        var charge = mode;
                        break;
                    }
            }
        }


        public void SetMode(byte index)
        {
            var data = new byte[2] { (byte)Command.SETMODE, index };

            StrikerVRConnectionManager.Instance.Send((System.Int16)address, data);
        }


        public void SetAmmo(int ammo)
        {
            ammo = Mathf.Clamp(ammo, 0, MAXAMMO);

            var data = new byte[2] { (byte)Command.SETAMMO, (byte)ammo };

            StrikerVRConnectionManager.Instance.Send((System.Int16)address, data);
        }
    }
}
