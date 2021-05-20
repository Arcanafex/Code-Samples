using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Spaces.Core;

namespace Spaces.UnityClient
{
    public class LoginInterface : MonoBehaviour
    {
        [System.Serializable]
        public class LoginPanelSet
        {
            public GameObject UserSelectionPanel;
            public GameObject UserAuthenticationPanel;
            public GameObject UserRegistrationPanel;

            [Space(2)]
            public GameObject LoginButton;
            public GameObject RegisterButton;
            public GameObject OKButton;

            [Space(2)]
            public GameObject RefreshButton;

        }

        public LoginPanelSet LoginPanels;

        public enum LoginDisplayState
        {
            Login,
            Registration,
            Select
        }

        private LoginDisplayState displayState;
        public LoginDisplayState DisplayState
        {
            get { return displayState; }
            set
            {
                displayState = value;
                if (displayState == LoginDisplayState.Select)
                {
                    LoginPanels.UserAuthenticationPanel.SetActive(false);
                    LoginPanels.LoginButton.SetActive(false);

                    LoginPanels.UserRegistrationPanel.SetActive(false);
                    LoginPanels.RegisterButton.SetActive(false);

                    LoginPanels.UserSelectionPanel.SetActive(true);
                    LoginPanels.OKButton.SetActive(true);
                    LoginPanels.RefreshButton.SetActive(true);
                }
                else if (displayState == LoginDisplayState.Login)
                {
                    LoginPanels.UserAuthenticationPanel.SetActive(true);
                    LoginPanels.LoginButton.SetActive(true);

                    LoginPanels.UserRegistrationPanel.SetActive(false);
                    LoginPanels.RegisterButton.SetActive(false);

                    LoginPanels.UserSelectionPanel.SetActive(false);
                    LoginPanels.OKButton.SetActive(false);
                    LoginPanels.RefreshButton.SetActive(false);

                }
                else if (displayState == LoginDisplayState.Registration)
                {
                    LoginPanels.UserAuthenticationPanel.SetActive(false);
                    LoginPanels.LoginButton.SetActive(false);

                    LoginPanels.UserRegistrationPanel.SetActive(true);
                    LoginPanels.RegisterButton.SetActive(true);

                    LoginPanels.UserSelectionPanel.SetActive(false);
                    LoginPanels.OKButton.SetActive(false);
                    LoginPanels.RefreshButton.SetActive(false);

                }
            }
        }


        public VerticalLayoutGroup UserPanel;
        public GameObject UserEntryPrefab;

        //[System.Serializable]
        //public class ConnectionStatusDisplay
        //{
        //    public Image statusImage;
        //    public Sprite onlineImage;
        //    public Sprite offlineImage;
        //    public Sprite reconnectImage;
        //}

        //public ConnectionStatusDisplay connectionStatus;

        [System.Serializable]
        public class LoginFieldReferences
        {
            public InputField userName;
            public InputField firstName;
            public InputField lastName;
            public InputField password;

        }

        public LoginFieldReferences RegisterPanelInputFields;
        public LoginFieldReferences loginPanelInputFields;

        void OnEnable()
        {
            //UpdateUserMode();
        }

        void Start()
        {
            UpdateStatus();
        }

        void UpdateUserMode()
        {
            DisplayState = LoginDisplayState.Select;
            InitializeUserEntries();
        }

        public void Refresh()
        {
            if (DisplayState == LoginDisplayState.Select)
                InitializeUserEntries();
        }

        #region UserPanel

        void InitializeUserEntries()
        {
            if (UserSession.Instance.Users == null)
            {
                DisplayState = LoginDisplayState.Login;
            }
            else
            {
                if (UserSession.Instance.Users.Count == 1)
                {
                    //UserSession.Instance.SetCurrentUser(UserSession.Instance.Users[0]);
                    Close();
                    return;
                }

                foreach (Transform child in UserPanel.transform)
                    Destroy(child.gameObject);

                var defaultIndex = 0; // UserSession.Instance.GetDefaultUserIndex();
                var toggleGroup = UserPanel.GetComponent<ToggleGroup>();
                var UserList = UserSession.Instance.Users;

                //foreach (AllUsers user in UserList) // foreach loops don't play nice with variables in delegates as UnityEvent listeners
                for (int u = 0; u < UserList.Count; u++)
                {
                    Spaces.Core.User user = UserList[u];

                    GameObject userEntry = Instantiate(UserEntryPrefab);
                    Toggle userToggle = userEntry.GetComponent<Toggle>();
                    Text label = userEntry.GetComponentInChildren<Text>();

                    userEntry.transform.SetParent(UserPanel.transform, false);

                    if (label)
                        label.text = user.name;

                    if (userToggle)
                    {
                        userToggle.onValueChanged.AddListener(delegate { User_Click(userToggle, user); });
                        userToggle.group = toggleGroup;

                        if (u == defaultIndex)
                        {
                            userToggle.isOn = true;
                        }
                    }
                }
            }
        }

        void User_Click(Toggle toggle, Spaces.Core.User user)
        {
            if (!UserSession.Instance.Users.Contains(user) || !toggle.isOn)
                return;

            if (Debug.isDebugBuild)
                Debug.Log(this.name + " A user by the name of " + user.name + " just got clicked!");

            //UserSession.Instance.SetCurrentUser(user);
        }

        #endregion

        private void UpdateStatus()
        {
            DisplayState = LoginDisplayState.Login;//.Registration;
        }

        public void ReloadUserList()
        {
            //UserSession.Instance.UpdateUserList();
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        public void UserRegistration()
        {
            DisplayState = LoginDisplayState.Registration;
        }

        public void RegisterNewUser()
        {
            User.CreateUser(
                RegisterPanelInputFields.userName.text,
                RegisterPanelInputFields.userName.text + "@space.com",
                RegisterPanelInputFields.firstName.text,
                RegisterPanelInputFields.lastName.text,
                "Q",
                RegisterPanelInputFields.password.text
                );

        }

        public void Login()
        {
            UserSession.Instance.Login(loginPanelInputFields.userName.text, loginPanelInputFields.password.text);

            //UserSession.Instance.CurrentUser.onTokenUpdated += delegate (Core.User user) { if (user.authenticated) Logout(); };
        }

        public void Logout()
        {
            UserSession.Instance.Logout();
        }
    }
}