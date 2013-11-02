using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Fiddler.Extensions
{
    public class RequestHelper : IAutoTamper
    {
        #region Private variables
        #region UI
        private MenuItem _miBlockThisRequest;
        private MenuItem _mnuRequestHelper;
        private MenuItem _miChangeDomainTo;
        private MenuItem _miRequestHelperEnabled;
        #endregion

        private bool _isRequestHelperEnabled = false;
        private List<string> UrlsToBlock = new List<string>();
        private Dictionary<string, string> UrlDomainMapping = new Dictionary<string, string>(); 
        #endregion

        #region Constructor and UI initializers
        /// <summary>
        /// Constructor
        /// </summary>
        public RequestHelper()
        {
            InitializeMenu();
        }

        /// <summary>
        /// Initialize the Main menu and context menu items
        /// </summary>
        private void InitializeMenu()
        {
            //Initialize menu instances
            _miBlockThisRequest = new MenuItem();
            _mnuRequestHelper = new MenuItem();
            _miChangeDomainTo = new MenuItem();
            _miRequestHelperEnabled = new MenuItem();

            //Main Menu
            _mnuRequestHelper.MenuItems.AddRange(new MenuItem[] {
                                                                  _miRequestHelperEnabled
                                                        });

            _mnuRequestHelper.Text = "&Request Helper";

            //Main menu Item 1 Initialize
            _miRequestHelperEnabled.Index = 0;
            _miRequestHelperEnabled.Text = "&Enabled";
            _miRequestHelperEnabled.Click += new EventHandler(EnableButton_Click);



            //Context Menu

            //Block this request context menu
            _miBlockThisRequest.Text = "&Block this URL";
            _miBlockThisRequest.Click += new System.EventHandler(BlockThisRequestContextItem_Click);

            //Change the domain context menu
            _miChangeDomainTo.Text = "&Change domain to";
            _miChangeDomainTo.Click += new System.EventHandler(ChangeDomainToContextItem_Click);
        } 
        #endregion

        #region Event Handlers
        /// <summary>
        /// Enable/Disable this extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableButton_Click(object sender, System.EventArgs e)
        {
            MenuItem oSender = (sender as MenuItem);
            oSender.Checked = !oSender.Checked;

            _isRequestHelperEnabled = _miRequestHelperEnabled.Checked;
            if (_isRequestHelperEnabled)
            {
                _mnuRequestHelper.Text = "&Request Helper-ON";
            }
            else
            {
                _mnuRequestHelper.Text = "&Request Helper";
            }

            //TODO : Based on enable/disable show/hide other menu items in the main menu
            // Enable menuitems based on overall enabled state.
            //miEditBlockedHosts.Enabled = miFlashAlwaysBlock.Enabled = miShortCircuitRedirects.Enabled = miLikelyPaths.Enabled = miHideBlockedSessions.Enabled =
            //miBlockXDomainFlash.Enabled = miSplit2.Enabled = bBlockerEnabled;
        }



        /// <summary>
        /// Event handler for Change domain to context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeDomainToContextItem_Click(object sender, System.EventArgs e)
        {
            Session[] oSessions = FiddlerApplication.UI.GetSelectedSessions();
            foreach (Session oSession in oSessions)
            {
                try
                {
                    string domain = frmPrompt.GetUserString("Domain to Switch", "Enter the domain to switch for the request : " + oSession.url, "");
                    MapUrltoDomain(oSession, domain);
                }
                catch (Exception eX)
                {
                    MessageBox.Show(eX.Message, "Cannot block host");
                }
            }
        }


        /// <summary>
        /// Event handler for Block this URL context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlockThisRequestContextItem_Click(object sender, System.EventArgs e)
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
        #endregion

        #region Configuration Methods
        /// <summary>
        /// Adds the session url and the redirect domain to the UrlDomainMapping dictionary
        /// </summary>
        /// <param name="oSession">Fiddler Session</param>
        /// <param name="domain">Domain to remap</param>
        private void MapUrltoDomain(Session oSession, string domain)
        {
            if (!UrlDomainMapping.ContainsKey(oSession.url.ToLower()))
            {
                UrlDomainMapping[oSession.url.ToLower()] = domain.ToLower();
            }
        }


        /// <summary>
        /// Adds the current session url to the Blocked urls list
        /// </summary>
        /// <param name="oSession"></param>
        private void AddToBlockList(Session oSession)
        {
            if (!UrlsToBlock.Contains(oSession.url.ToLower()))
            {
                UrlsToBlock.Add(oSession.url.ToLower());
            }
        } 
        #endregion

        #region Functional Methods
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

        private void RedirectSession(Session oSession)
        {
            oSession.host = UrlDomainMapping[oSession.url.ToLower()];
        } 
        #endregion

        #region IAutoTamper Methods
        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
            //Load the Main menu and context menu here
            FiddlerApplication.UI.mnuMain.MenuItems.Add(_mnuRequestHelper);
            FiddlerApplication.UI.mnuSessionContext.MenuItems.Add(0, _miBlockThisRequest);
            FiddlerApplication.UI.mnuSessionContext.MenuItems.Add(1, _miChangeDomainTo);
        }

        public void AutoTamperRequestAfter(Session oSession)
        {
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (!_isRequestHelperEnabled) return;

            if (UrlsToBlock.Contains(oSession.url.ToLower()))
            {
                BlockThisSession(oSession);
            }
            if (UrlDomainMapping.ContainsKey(oSession.url.ToLower()))
            {
                RedirectSession(oSession);
            }
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
        #endregion
    }
}
