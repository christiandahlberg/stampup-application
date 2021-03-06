﻿using PK1_Client.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Resources.Models;

namespace PK1_Client.GUI


{
    public partial class SubscriptionsGUI : Form
    {
        private CustomerController customerController;
        private StoreController storeController;
        private OfferController offerController;

        public SubscriptionsGUI()
        {
            InitializeComponent();

            customerController = new CustomerController();
            storeController = new StoreController();
            offerController = new OfferController();

            SetCustomerContent();
            SetStoreContent();
        }

        // ---------------------------------------------------------------------------
        // --------------------------------- MISC ------------------------------------
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Fills customer combobox with all customers.
        /// </summary>
        private void SetCustomerContent()
        {
            foreach (Customer c in customerController.GetAllCustomers())
            {
                var customer = new ComboBoxItem<int>
                {
                    DisplayMember = c.Name,
                    ValueMember = c.ID
                };
                comboBox_Customer.Items.Add(customer);
            }
        }

        /// <summary>
        /// Fills store combobox with all stores.
        /// </summary>
        private void SetStoreContent()
        {
            foreach (Store s in storeController.GetAllStores())
            {
                comboBox_Store.Items.Add(s.Name);
            }
        }

        /// <summary>
        /// Shows offers depending on store selection. Also shows what offers current selected customer are subscribed to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectedStore_Click(object sender, EventArgs e)
        {
            // Clears
            dgv_Offers.Rows.Clear();
            setAddStampFalse();
            SetSystemMessage("");
            
            foreach (Offer o in offerController.GetAllOffers())
            {
                if (o.Store.Name.Equals(comboBox_Store.SelectedItem.ToString()))
                {
                    int rowId = dgv_Offers.Rows.Add();
                    DataGridViewRow row = dgv_Offers.Rows[rowId];

                    row.Cells[0].Value = o.ID;
                    row.Cells[1].Value = o.Name;
                    row.Cells[2].Value = o.Description;

                    int i = ((ComboBoxItem<int>)comboBox_Customer.SelectedItem).ValueMember;
                    foreach (Offer offer in offerController.GetCustomerOffersById(i))
                    {
                        if (o.ID == offer.ID)
                        {
                            DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)row.Cells[3];
                            cell.Value = true;
                        }
                    }
                    dgv_Offers.ClearSelection();
                }
            }

        }

        private void setAddStampFalse()
        {
            btnAddStamp.BackColor = Color.Gray;
            btnAddStamp.Enabled = false;
        }

        // Sets system message depending on error or successful
        public void SetSystemMessage(string message)
        {
            if (message.Contains("ERROR:"))
            {
                lbl_SystemMessage.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lbl_SystemMessage.ForeColor = System.Drawing.Color.Black;
            }
            lbl_SystemMessage.Text = message;
        }

        /// <summary>
        /// HANDLE ADD / DELETE SUBSCRIPTIONS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void subscribed_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex != -1)
            {
                int oId = (int)dgv_Offers.Rows[e.RowIndex].Cells[0].Value;
                int cId = ((ComboBoxItem<int>)comboBox_Customer.SelectedItem).ValueMember;

                bool isChecked = Convert.ToBoolean(dgv_Offers[e.ColumnIndex, e.RowIndex].Value);
                dgv_Offers.Rows[e.RowIndex].DefaultCellStyle.BackColor = isChecked ? Color.LightGreen : Color.White;

                if (isChecked)
                {
                    // See if sub is already active (fixing error)
                    if (offerController.FindSubscription(oId, cId))
                    {
                        return;
                    }
                    else
                    {
                        // Add subscription if sub is not already active
                        if (offerController.AddSubscription(oId, cId))
                        {
                            SetSystemMessage("Successfully subscribed.");
                        }
                    }
                }
                else
                {
                    if (offerController.RemoveSubscription(offerController.GetOfferByOfferID(oId), cId))
                    {
                        SetSystemMessage("Successfully removed subscription.");
                    }
                }

            }
        }

        /// <summary>
        /// Workaround event for executing logics in subscribed_CellValueChanged without constraints.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void subscribed_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex != -1)
            {
                dgv_Offers.EndEdit();
                dgv_Offers.ClearSelection();
            }
        }

        private void refreshTable(object sender, EventArgs e)
        {
            if (comboBox_Store.SelectedItem != null)
            {
                setAddStampFalse();
                selectedStore_Click(sender, e);
            }
        }

        private void btnAddStamp_Click(object sender, EventArgs e)
        {
            if (comboBox_Customer.SelectedItem == null)
            {
                lbl_SystemMessage.Text = "Please choose a customer.";
            } else if (comboBox_Store.SelectedItem == null)
            {
                lbl_SystemMessage.Text = "Please choose a store.";
            } else
            {
                int offerId = (int)dgv_Offers.Rows[dgv_Offers.CurrentCell.RowIndex].Cells[0].Value;
                Offer o = offerController.GetOfferByOfferID(offerId);
                int current_stamps = customerController.GetStampsAttained(((ComboBoxItem<int>)comboBox_Customer.SelectedItem).ValueMember, offerId);

                if (offerController.FindSubscription(offerId, ((ComboBoxItem<int>)comboBox_Customer.SelectedItem).ValueMember))
                {
                    if (customerController.IncrementStampsAttained(((ComboBoxItem<int>)comboBox_Customer.SelectedItem).ValueMember, offerId))
                    {
                        current_stamps++;
                        lbl_SystemMessage.Text = $"Stamp added. Current stamp on {o.Name}: {current_stamps} of {o.StampGoal}.";
                    }
                    else
                    {
                        lbl_SystemMessage.Text = $"Stamp goal achieved ({current_stamps} of {o.StampGoal}).";
                    }
                }
                else
                {
                    lbl_SystemMessage.Text = "Please choose a subscription to increment stamps for.";
                }
            }
        }

        private void setAddStampTrue(object sender, DataGridViewCellEventArgs e)
        {
            btnAddStamp.Enabled = true;
            btnAddStamp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));

        }
    }

    /// <summary>
    /// Custom class to handle objects in combobox to display and return different values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ComboBoxItem<T>
    {
        public string DisplayMember = string.Empty;
        public T ValueMember = default(T);

        public override string ToString()
        {
            return DisplayMember;
        }
    }
}
