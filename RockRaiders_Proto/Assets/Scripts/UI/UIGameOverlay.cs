using Assets.Scripts.Actor;
using Assets.Scripts.Match;
using Assets.Scripts.Network;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using System.Diagnostics;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIGameOverlay : MonoBehaviour
    {
        private GameManager m_gameManager;
        private MatchManager m_matchManager;

        [SerializeField]
        private GameObject m_hud;

        [SerializeField]
        private GameObject m_ded;

        [SerializeField]
        private GameObject m_pending;

        [SerializeField]
        private GameObject m_pauseMenu;

        [SerializeField]
        private GameObject m_deathmatchScore;

        [SerializeField]
        private GameObject m_teamScore;

        [SerializeField]
        private GameObject m_matchOverDm;

        [SerializeField]
        private GameObject m_matchOverTeam;

        [SerializeField]
        private ActorController m_actor;


        private Text m_txtGravBootsEnabled;
        private Text m_txtHitpointsValue;
        private Text m_txtRedScore;
        private Text m_txtBlueScore;
        private Text m_txtTeamScoreLimit;
        private Text m_txtScoreLimit;
        private Text m_txtScoreValue;
        private Text m_txtAmmoCountValue;
        private TMP_Text m_txtNotificationText;
        private Timer m_notificationTextBumpTimer;
        private Timer m_hitMarkerTimer;

        private GameObject m_hitMarker;

        private ActorState m_actorState;
        private IReadonlyMatchData m_matchData;

        public ActorController Actor
        {
            get
            {
                return m_actor;
            }
            set
            {
                m_actor = value;
                m_actorState = m_actor.GetComponent<ActorState>();
            }
        }

        public UIGameOverlay()
        {
            m_notificationTextBumpTimer = new Timer(TimeSpan.FromSeconds(5));
            m_notificationTextBumpTimer.OnTimerElapsed += NotificationTextBumpTimer_OnTimerElapsed;

            m_hitMarkerTimer = new Timer(TimeSpan.FromSeconds(0.5));
            m_hitMarkerTimer.OnTimerElapsed += HitMarkerTimer_OnTimerElapsed;
            m_hitMarkerTimer.AutoReset = false;
        }

        private void HitMarkerTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            m_hitMarker.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (m_gameManager == null)
            {
                m_gameManager = GameManager.Instance;
            }

            if (m_matchManager == null && MatchManager.Instance != null)
            {
                m_matchManager = MatchManager.Instance;
            }

            if (m_matchManager == null)
            {
                throw new NullReferenceException(nameof(m_matchManager));
            }

            m_matchManager.OnMatchStateChanged += this.MatchManager_OnMatchStateChanged;
            NotificationService.Instance.OnPlayerKilled += this.NotificationService_OnPlayerKilled;
            NotificationService.Instance.OnPlayerAttacked += this.NotificationService_OnPlayerAttacked;

            m_txtAmmoCountValue = m_hud.FindChild("Ammo.AmmoValue").GetComponent<Text>();
            m_txtGravBootsEnabled = m_hud.FindChild("GravBoots.GravbootsValue").GetComponent<Text>();
            m_txtHitpointsValue = m_hud.FindChild("HitPoints.HitpointsValue").GetComponent<Text>();


            m_txtRedScore = m_teamScore.FindChild("RedScoreValue").GetComponent<Text>();
            m_txtBlueScore = m_teamScore.FindChild("BlueScoreValue").GetComponent<Text>();
            m_txtScoreValue = m_deathmatchScore.FindChild("ScoreValue").GetComponent<Text>();
            m_txtTeamScoreLimit = m_teamScore.FindChild("ScoreLimitValue").GetComponent<Text>();
            m_txtScoreLimit = m_deathmatchScore.FindChild("ScoreLimitValue").GetComponent<Text>();

            m_hitMarker = m_hud.FindChild("HitMarker");

            m_txtNotificationText = m_hud.FindChild("NotificationText").GetComponent<TMP_Text>();
            m_txtNotificationText.text = string.Empty;

            m_notificationTextBumpTimer.Start();
        }

        private void NotificationService_OnPlayerAttacked(object sender, Events.OnNotificationEventArgs e)
        {
            var data = e.Data.GetData<PlayerAttackedData>();
            var actNet = data.Attacker.GetComponent<ActorNetwork>();

            if (actNet != null)
            {
                var isAtkLocal = data.Attacker.GetComponent<ActorNetwork>().IsLocalPlayer;

                if (isAtkLocal)
                {
                    m_hitMarkerTimer.ResetTimer();
                    m_hitMarkerTimer.Start();

                    if (m_hitMarker != null)
                    {
                        m_hitMarker.SetActive(true);
                    }
                }
            }
            else
            {
                Debugger.Break();
            }

        }

        private void MatchManager_OnMatchStateChanged(object sender, System.EventArgs e)
        {

        }

        public void DisplayMatchOverDM(IReadonlyMatchData matchData)
        {
            m_matchOverDm.SetActive(true);

            var txtPlayerName = m_matchOverDm.FindChild("PlayerNameText");
            var txt = txtPlayerName.GetComponent<TMP_Text>();

            if (matchData.Teams.ContainsKey(Team.None))
            {
                var winner = matchData.GetTopPlayerByTeam(Team.None);
                var wState = winner.Player.GetComponent<ActorState>();

                txt.text = wState.PlayerName;
            }
            else
            {
                txt.text = "Nobody";
            }

        }

        public void DisplayMatchOverTeam(IReadonlyMatchData matchData)
        {
            m_matchOverTeam.SetActive(true);

            var winner = matchData.Teams
                .Where(x => x.Value.Team != Team.None)
                .OrderByDescending(x => x.Value.TeamScore)
                .First();

            var txtTeamName = m_matchOverTeam.FindChild("TeamNameText");
            var txt = txtTeamName.GetComponent<TMP_Text>();
            txt.text = winner.Value.Team.ToString();
            txt.color = winner.Value.Team == Team.Blue ? Color.blue : Color.red;
        }

        private void DisplayOverlay()
        {
            if (m_matchData == null || !m_matchManager.IsReady)
            {
                return;
            }

            switch (m_matchData.MatchState)
            {
                case MatchState.PendingStart:
                    m_hud.SetActive(false);
                    m_matchOverTeam.SetActive(false);
                    m_pending.SetActive(!m_gameManager.PlayerPaused);
                    m_pauseMenu.SetActive(m_gameManager.PlayerPaused);
                    break;
                case MatchState.Running:
                    this.UpdateGravBootsStatus();
                    this.UpdateHealthStatus();
                    this.UpdateAmmoStatus();

                    m_matchOverTeam.SetActive(false);

                    if (m_gameManager.PlayerPaused)
                    {
                        m_hud.SetActive(false);
                        m_pending.SetActive(false);
                        m_pauseMenu.SetActive(true);
                    }
                    else if (m_gameManager.LocalPlayerAwaitingRespawn)
                    {
                        m_ded.SetActive(true);
                        m_hud.SetActive(false);
                    }
                    else
                    {
                        m_hud.SetActive(true);
                        m_ded.SetActive(false);
                        m_pending.SetActive(false);
                        m_pauseMenu.SetActive(false);
                    }

                    switch (m_matchData.MatchType)
                    {
                        case MatchType.Deathmatch:
                            m_deathmatchScore.SetActive(true);
                            m_teamScore.SetActive(false);
                            this.UpdatePlayerScore();
                            break;
                        case MatchType.TeamDeathmatch:
                        case MatchType.CaptureTheFlag:
                            m_deathmatchScore.SetActive(false);
                            m_teamScore.SetActive(true);
                            this.UpdateTeamScore();
                            break;
                    }

                    break;
                case MatchState.Ended:
                    m_ded.SetActive(false);
                    m_hud.SetActive(false);
                    m_pauseMenu.SetActive(m_gameManager.PlayerPaused);

                    switch (m_matchData.MatchType)
                    {
                        case MatchType.Deathmatch:
                            this.DisplayMatchOverDM(m_matchData);
                            break;
                        case MatchType.TeamDeathmatch:
                        case MatchType.CaptureTheFlag:
                            this.DisplayMatchOverTeam(m_matchData);
                            break;
                    }
                    break;
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (m_matchManager?.IsReady ?? false)
            {
                m_matchData = m_matchManager.GetMatchData();
            }

            m_notificationTextBumpTimer.Tick();
            m_hitMarkerTimer.Tick();
        }

        private void OnGUI()
        {
            this.DisplayOverlay();
        }

        private void UpdateTeamScore()
        {
            if (m_matchData == null)
            {
                return;
            }

            var blueTeamScore = m_matchData.Teams[Team.Blue].TeamScore;
            var redTeamScore = m_matchData.Teams[Team.Red].TeamScore;
            var scoreLimit = m_matchData.ScoreLimit;

            m_txtRedScore.text = redTeamScore.ToString();
            m_txtBlueScore.text = blueTeamScore.ToString();
            m_txtTeamScoreLimit.text = scoreLimit.ToString();
        }

        private void UpdatePlayerScore()
        {
            if (m_matchData == null)
            {
                return;
            }

            var currClientId = NetworkManager.Singleton.LocalClientId;

            var players = m_matchData.Teams[Team.None].Players;

            m_txtScoreLimit.text = m_matchData.ScoreLimit.ToString();

            if (!players.ContainsKey(currClientId))
            {
                m_txtScoreValue.text = "UNKNOWN";
                NotificationService.Instance.Warning($"No player with client id {currClientId} could be found");
                return;
            }

            var currPlayer = m_matchData.Teams[Team.None].Players[currClientId];
            var score = currPlayer.Score;

            m_txtScoreValue.text = score.ToString();
            
        }

        private void UpdateGravBootsStatus()
        {
            string txtVal = "Unknown";
            txtVal = m_actorState?.GravBootsEnabled ?? false ? "True" : "False";
            var txtTint = m_actorState?.GravBootsEnabled ?? false ? Color.green : Color.red;
            m_txtGravBootsEnabled.text = txtVal;
            m_txtGravBootsEnabled.color = txtTint;
        }

        private void UpdateHealthStatus()
        {
            var hp = m_actorState?.Health ?? -1;
            var txtTint = GetColourFromPercentage(hp);
            m_txtHitpointsValue.text = hp > 0 ? hp.ToString() + "%" : "Unknown";
            m_txtHitpointsValue.color = txtTint;
        }

        private Color GetColourFromPercentage(float percentage)
        {
            if (percentage < 30)
            {
                return Color.red;
            }

            if (percentage < 50)
            {
                return Color.yellow;
            }

            return Color.green;
        }

        private void UpdateAmmoStatus()
        {
            if (m_actorState == null)
            {
                return;
            }

            var weaponObj = m_actorState.Inventory.GetSelectedWeapon();
            var ammoCount = -1;
            var maxAmmo = 0;
            var ammoPercentage = 0.0f;

            if (weaponObj != null)
            {
                var wpn = weaponObj.GetComponent<Weapon>();
                ammoCount = wpn.Ammo;
                ammoPercentage = ((float)wpn.Ammo / (float)wpn.MaxAmmo) * 100.0f;
                maxAmmo = wpn.MaxAmmo;
            }

            m_txtAmmoCountValue.text = ammoCount > -1 ? ammoCount.ToString() : maxAmmo == -1 ? "Infinite" : "Unknown";
            m_txtAmmoCountValue.color = this.GetColourFromPercentage(ammoPercentage);
        }

        private void NotificationService_OnPlayerKilled(object sender, Events.OnNotificationEventArgs e)
        {
            m_txtNotificationText.text = e.Data.Message + "\n" + m_txtNotificationText.text;
        }

        private void NotificationTextBumpTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            m_txtNotificationText.text = "\n" + m_txtNotificationText.text;
        }

    }
}