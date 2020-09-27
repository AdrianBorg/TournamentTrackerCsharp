using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        List<int> rounds = new List<int>();
        List<MatchupModel> selectedMatchups = new List<MatchupModel>();
        public TournamentViewerForm(TournamentModel tournamentModel)
        {
            InitializeComponent();

            tournament = tournamentModel;

            LoadFormData();

            LoadRounds();
        }

        private void LoadFormData()
		{
            tournamentName.Text = tournament.TournamentName;
		}

        private void WireUpRoundsList()
		{
            roundDropDown.DataSource = rounds;
		}

        private void WireUpMatchupsList()
        {
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }

        private void LoadRounds()
		{
            rounds = new List<int>();

            rounds.Add(1);
            int currRound = 1;

            tournament.Rounds.ForEach(matchups =>
            {
				if (matchups.First().MatchupRound > currRound)
				{
					currRound = matchups.First().MatchupRound;
                    rounds.Add(currRound);
				}
            });

            WireUpRoundsList();
		}

		private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
            LoadMatchups();
		}

        private void LoadMatchups()
		{
            int round = (int)roundDropDown.SelectedItem;

            tournament.Rounds.ForEach(matchups =>
            {
                if (matchups.First().MatchupRound == round)
                {
                    selectedMatchups = matchups.Where(x => x.Winner == null || !unplayedOnlyCheckbox.Checked).ToList();
                }
            });

            WireUpMatchupsList();

            DisplayMatchupInfo();
        }

        private void DisplayMatchupInfo()
		{
            bool isVisible = selectedMatchups.Count > 0;

            teamOneName.Visible = isVisible;
            teamTwoName.Visible = isVisible;
            teamOneScoreLabel.Visible = isVisible;
            teamTwoScoreLabel.Visible = isVisible;
            teamOneScoreValue.Visible = isVisible;
            teamTwoScoreValue.Visible = isVisible;
            versusLabel.Visible = isVisible;
            scoreButton.Visible = isVisible;
		}

        private void LoadMatchup()
		{
            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;

			for (int i = 0; i < m.Entries.Count; i++)
			{
				if (i == 0)
				{
					if (m.Entries[0].TeamCompeting != null)
					{
						teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                        teamOneScoreValue.Text = m.Entries[0].Score.ToString();

                        teamTwoName.Text = "<bye>";
                        teamTwoScoreValue.Text = "0";
                    }
					else
					{
                        teamOneName.Text = "Not yet set";
                        teamOneScoreValue.Text = "";
                    }
                }

                if (i == 1)
                {
                    if (m.Entries[1].TeamCompeting != null)
                    {
                        teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                        teamTwoScoreValue.Text = m.Entries[1].Score.ToString();
                    }
                    else
                    {
                        teamTwoName.Text = "Not yet set";
                        teamTwoScoreValue.Text = "";
                    }
                }
            }
		}

		private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
            LoadMatchup();
		}

		private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
		{
            LoadMatchups();
        }

		private void scoreButton_Click(object sender, EventArgs e)
		{
            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;
            double teamOneScore = 0;
            double teamTwoScore = 0;

            for (int i = 0; i < m.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        bool scoreValid = double.TryParse(teamOneScoreValue.Text, out teamOneScore);

						if (scoreValid)
						{
							m.Entries[0].Score = teamOneScore;
						}
						else
						{
                            MessageBox.Show("Enter a valid score for team 1.");
                            return;
						}
                    }
                }
                if (i == 1)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        bool scoreValid = double.TryParse(teamTwoScoreValue.Text, out teamTwoScore);

                        if (scoreValid)
                        {
                            m.Entries[1].Score = teamTwoScore;
                        }
                        else
                        {
                            MessageBox.Show("Enter a valid score for team 2.");
                            return;
                        }
                    }
                }
            }

            TournamentLogic.UpdateTournamentResults(tournament);

            LoadMatchups();
        }
	}
}
