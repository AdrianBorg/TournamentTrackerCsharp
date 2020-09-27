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
	public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
	{
		ITournamentRequester callingForm;
		List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeams_All();
		List<TeamModel> selectedTeams = new List<TeamModel>();
		List<PrizeModel> selectedPrizes = new List<PrizeModel>();

		public CreateTournamentForm(ITournamentRequester caller)
		{
			callingForm = caller;

			InitializeComponent();

			WireUpLists();
		}

		private void WireUpLists()
		{

			selectTeamDropdown.DataSource = null;
			selectTeamDropdown.DataSource = availableTeams;
			selectTeamDropdown.DisplayMember = "TeamName";

			tournamentTeamsListBox.DataSource = null;
			tournamentTeamsListBox.DataSource = selectedTeams;
			tournamentTeamsListBox.DisplayMember = "TeamName";

			prizesListBox.DataSource = null;
			prizesListBox.DataSource = selectedPrizes;
			prizesListBox.DisplayMember = "PlaceName";
		}

		private void addTeamButton_Click(object sender, EventArgs e)
		{
			TeamModel t = (TeamModel)selectTeamDropdown.SelectedItem;

			if (t != null)
			{
				availableTeams.Remove(t);
				selectedTeams.Add(t);

				WireUpLists();
			}
		}

		private void createPrizeButton_Click(object sender, EventArgs e)
		{
			CreatePrizeForm frm = new CreatePrizeForm(this);
			frm.Show();
		}

		public void PrizeComplete(PrizeModel model)
		{
			selectedPrizes.Add(model);
			WireUpLists();
		}

		public void TeamComplete(TeamModel model)
		{
			selectedTeams.Add(model);
			WireUpLists();
		}

		private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CreateTeamForm frm = new CreateTeamForm(this);
			frm.Show();
		}

		private void removeSelectedPlayerButton_Click(object sender, EventArgs e)
		{
			TeamModel t = (TeamModel)tournamentTeamsListBox.SelectedItem;

			if (t != null)
			{
				selectedTeams.Remove(t);
				availableTeams.Add(t);

				WireUpLists();
			}
		}

		private void removeSelectedPrizeButton_Click(object sender, EventArgs e)
		{
			PrizeModel p = (PrizeModel)prizesListBox.SelectedItem;
			if (p != null)
			{
				selectedPrizes.Remove(p);

				WireUpLists();
			}
		}

		private void createTournamentButton_Click(object sender, EventArgs e)
		{
			// Validate data

			bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out decimal fee);

			if (!feeAcceptable)
			{
				MessageBox.Show(
					"Entry fee Invalid", 
					"Invalid fee", 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Error
					);
				return;
			}

			// Create oturnament model
			TournamentModel tm = new TournamentModel();
			tm.TournamentName = tournamentNameValue.Text;
			tm.EntryFee = fee;

			tm.Prizes = selectedPrizes;
			tm.EnteredTeams = selectedTeams;

			// TODO - Wire our matchups
			TournamentLogic.CreateRounds(tm);

			// Create tournament ID
			// Create all of the prizes entries
			// Create all of the team entries

			GlobalConfig.Connection.CreateTournament(tm);

			callingForm.TournamentComplete();
			TournamentViewerForm frm = new TournamentViewerForm(tm);
			frm.Show();
			this.Close();
		}
	}
}
