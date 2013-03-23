using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Fiddler.Extensions
{
    public class RequestHelper : IAutoTamper
    {
        #region UI
        private MenuItem miBlockThisRequest;
        private MenuItem mnuRequestHelper;
        private MenuItem miChangeDomainTo;
        #endregion

        private List<string> UrlsToBlock = new List<string>();
        private Dictionary<string, string> UrlDomainMapping = new Dictionary<string, string>();

        public RequestHelper()
        {
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            //Initialize menu instances
            miBlockThisRequest = new MenuItem();
            mnuRequestHelper = new MenuItem();
            miChangeDomainTo = new MenuItem();

            //Main Menu
            mnuRequestHelper.Text = "&Request Helper";
            //Context Menu

            //Block this request context menu
            miBlockThisRequest.Text = "&Block this URL";
            miBlockThisRequest.Click += new System.EventHandler(miBlockThisRequest_Click);

            //Change the domain context menu
            miChangeDomainTo.Text = "&Change domain to";
            miChangeDomainTo.Click += new System.EventHandler(miChangeDomainTo_Click);
        }

        /// <summary>
        /// Event handler for Change domain to context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miChangeDomainTo_Click(object sender, System.EventArgs e)
        {
            Session[] oSessions = FiddlerApplication.UI.GetSelectedSessions();
            foreach (Session oSession in oSessions)
            {
                try
                {
                    string domain = frmPrompt.GetUserString("Domain to Switch", "Enter the domain to switch for the request : " + oSession.url , "");
                    MapUrltoDomain(oSession, domain);
                }
                catch (Exception eX)
                {
                    MessageBox.Show(eX.Message, "Cannot block host");
                }
            }
        }

        private void MapUrltoDomain(Session oSession, string domain)
        {
            if (!UrlDomainMapping.ContainsKey(oSession.url.ToLower()))
            {
                UrlDomainMapping[oSession.url.ToLower()] = domain.ToLower();
            }
        }


        /// <summary>
        /// Event handler for Block this URL context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miBlockThisRequest_Click(object sender, System.EventArgs e)
        {
            Session[] oSessions = FiddlerApplication.UI.GetSelectedSessions();
            foreach (Session oSession in oSessions)
            {
                try
                {
                    AddToBlockList(oSession);
                }
                catch (Exception eX)
                {
                    MessageBox.Show(eX.Message, "Cannot block host");
                }
            }
        }


        private void AddToBlockList(Session oSession)
        {
            if (!UrlsToBlock.Contains(oSession.url.ToLower()))
            {
                UrlsToBlock.Add(oSession.url.ToLower());
            }
            StrikeOrHideSession(oSession);
        }


        private void BlockThisSession(Session oSession)
        {
            oSession.oRequest.FailSession(404, "Fiddler - Content Blocked", "Blocked this request");
            oSession.state = SessionStates.Done;
            StrikeOrHideSession(oSession);
        }

        private void StrikeOrHideSession(Session oSession)
        {
            oSession["ui-strikeout"] = "userblocked";
            oSession["ui-color"] = "gray";
        }
        

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
            //Load the Main menu and context menu here
            FiddlerApplication.UI.mnuMain.MenuItems.Add(mnuRequestHelper);
            FiddlerApplication.UI.mnuSessionContext.MenuItems.Add(0, miBlockThisRequest);
            FiddlerApplication.UI.mnuSessionContext.MenuItems.Add(1, miChangeDomainTo);
        }

        public void AutoTamperRequestAfter(Session oSession)
        {
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (UrlsToBlock.Contains(oSession.url))
            {
                BlockThisSession(oSession);
            }
            if (UrlDomainMapping.ContainsKey(oSession.url.ToLower()))
            {
                RedirectSession(oSession);
            }
        }

        private void RedirectSession(Session oSession)
        {
            oSession.host = UrlDomainMapping[oSession.url.ToLower()];
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
        }

        public void OnBeforeReturningError(Session oSession)
        {
        }
    }
}
