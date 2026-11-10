using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class LoginFailedPanel : WindowPanel {

        /*
        [SerializeField]
        private HighlightButton okButton = null;
        */

        [SerializeField]
        private TMP_Text failureMessage = null;

        // game manager references
        private UIManager uIManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private string defaultFailureText = null;
        private float defaultFontSize = 0f;
        private VerticalAlignmentOptions defaultAlignment;

        // 까닭 문구는 서너 줄이라 기본 한 줄 글자(50)로는 OK 단추를 덮는다 — 작게, 위에서부터 쓴다
        private const float reasonFontSize = 28f;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (failureMessage == null) {
                return;
            }
            if (defaultFailureText == null) {
                defaultFailureText = failureMessage.text;
                defaultFontSize = failureMessage.fontSize;
                defaultAlignment = failureMessage.verticalAlignment;
            }
            bool hasReason = string.IsNullOrEmpty(networkManagerClient.LoginFailureMessage) == false;
            failureMessage.text = hasReason ? networkManagerClient.LoginFailureMessage : defaultFailureText;
            failureMessage.fontSize = hasReason ? reasonFontSize : defaultFontSize;
            failureMessage.verticalAlignment = hasReason ? VerticalAlignmentOptions.Top : defaultAlignment;
        }

        public void ConfirmAction() {
            //Debug.Log("DisconnectedPanelController.ConfirmAction()");
            uIManager.loginFailedWindow.CloseWindow();
        }

    }

}