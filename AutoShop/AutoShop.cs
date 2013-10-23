using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace AutoShop
{
    [ApiVersion(1, 14)]
    public class AutoSell : TerrariaPlugin
    {

        #region Version, Name, Etc.
        public override Version Version
        {
            get
            {
                return new Version("0.6");
            }
        }
        public override string Name
        {
            get
            {
                return "AutoShop Plugin";
            }
        }
        public override string Author
        {
            get
            {
                return "beta) (By Snirk Immington";
            }
        }
        public override string Description
        {
            get
            {
                return "Buy and sell NPC goods with /buy and /sell!";
            }
        }
        public AutoSell(Main game)
            : base(game)
        {
            Order = 1;
        }
        #endregion

        public static string sellItem;
        public static int sellValue;

        public override void Initialize()
        {

            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }

        public static void OnInitialize(EventArgs args)
        {
            //Commands.ChatCommands.Add(new Command("autoshop", Buy, "buy"));
            Commands.ChatCommands.Add(new Command("price", Sell, "sell"));

        }

        public static void Sell(CommandArgs com)
        {
            if (com.Parameters.Count == 1)
            {
                #region price command
                if (com.Parameters[0].Equals("price"))
                {
                    if (com.TPlayer.inventory[0].value > 0 && com.TPlayer.inventory[0].maxStack > 0)
                    {
                        com.Player.SendMessage("Your " + com.TPlayer.inventory[0].name + " is worth " + ((com.TPlayer.inventory[0].value) / 5) + " coins!", Color.Green);
                        com.Player.SendMessage("If you would like to sell ONE " + com.TPlayer.inventory[0].name + ", type /sell confirm", Color.Green);
                        com.Player.SendMessage("You do not need to continue with purchase.", Color.Green);

                    } //end if exsists

                    else if (com.TPlayer.inventory[0].maxStack == 0)
                        com.Player.SendMessage("That is not a valid item!", Color.Red);

                    else if (com.TPlayer.inventory[0].value == 0)
                        com.Player.SendMessage(com.TPlayer.inventory[0].name + " has no value!", Color.Red);

                } //end if price
                #endregion

                #region set command
                if (com.Parameters[0].Equals("set") && com.Player.Group.HasPermission("autoshop"))
                {
                    if (!com.TPlayer.inventory[0].name.Equals(null) && !com.TPlayer.inventory[0].name.Contains("Coin") && com.TPlayer.inventory[0].value > 0)
                    {
                        sellItem = com.TPlayer.inventory[0].name;
                        sellValue = ((com.TPlayer.inventory[0].value) / 5);

                        com.Player.SendMessage("Now setting  \"" + sellItem + "\" as the item to sell for " + sellValue + " coins.", Color.Green);
                        com.Player.SendMessage("Drop the " + sellItem + " on the ground and don't move", Color.Yellow);
                        com.Player.SendMessage("Type /sell confirm to sell the item for ", Color.LightGreen);

                    } //end not coin, null

                    else if (com.TPlayer.inventory[0].name.Equals(null))
                        com.Player.SendMessage("That is not an item!", Color.Red);

                    else if (com.TPlayer.inventory[0].name.Contains("Coin"))
                        com.Player.SendMessage("Can't buy coins!", Color.Red);

                    else if (com.TPlayer.inventory[0].value == 0)
                        com.Player.SendMessage("That's worthless!", Color.Red);

                } //end set
                #endregion

                #region confirm command
                else if (com.Parameters[0].Equals("confirm") && com.Player.Group.HasPermission("autoshop"))
                {
                    com.Player.SendMessage("Selling " + sellItem + " on the ground near you...", Color.Red);

                    if (sellValue != 0 && !sellValue.Equals(null))
                    {
                        #region inventory check, if good
                        if (com.Player.InventorySlotAvailable && !sellItem.Equals(null) && !sellItem.Contains("coin"))
                        {
                            int copper = 0;
                            int silver = 0;
                            int gold = 0;
                            bool gotitem = false;

                            TSPlayer.All.SendMessage(com.Player.Name + "is using /sell, clearing dropped items near him/her. Do NOT interfere, begin a /sell, or approach!", Color.Red);

                            for (int j = 0; j < 200; j++)
                            {
                                #region if item dropped
                                if (
                    (Math.Sqrt(Math.Pow(Main.item[j].position.X - com.Player.X, 2) +
                               Math.Pow(Main.item[j].position.Y - com.Player.Y, 2)) < 7 * 16) && (Main.item[j].active))
                                //if (Main.item[i].position.X - com
                                {
                                    Main.item[j].active = false;
                                    NetMessage.SendData(0x15, -1, -1, "", j, 0f, 0f, 0f, 0);
                                    com.Player.SendMessage("Picked up " + sellItem + "!", Color.Green);
                                    gotitem = true;
                                    break; //found the item, break.

                                } //end of if item
                                #endregion //if item dropped

                            } //end for loop

                            #region if got item
                            if (gotitem)
                            {
                                com.Player.SendMessage("Giving you " + sellValue + " coins now.", Color.Green);

                                #region gold coin
                                while (sellValue >= 10000)
                                {
                                    sellValue -= 10000;
                                    gold++;
                                }
                                #endregion

                                #region silver
                                while (sellValue >= 100)
                                {
                                    sellValue -= 100;
                                    silver++;
                                }
                                #endregion

                                #region copper
                                while (sellValue > 0)
                                {
                                    sellValue--;
                                    copper++;
                                }
                                #endregion

                                if (gold > 0)
                                    com.Player.GiveItem(73, "Gold Coin", 0, 0, gold, 0);
                                if (silver > 0)
                                    com.Player.GiveItem(72, "Silver Coin", 0, 0, silver, 0);
                                if (copper > 0)
                                    com.Player.GiveItem(71, "Copper Coin", 0, 0, copper, 0);

                                com.Player.SendMessage("Gave Coins for " + sellItem, Color.Green);

                                TSPlayer.All.SendMessage("Transaction Complete!", Color.Green);

                                com.Player.SendMessage(gold + " Gold, " + silver + " Silver, " + copper + " copper.", Color.Green);

                                copper = silver = gold = sellValue = 0;
                                sellItem = null;

                            } //end of if dropped item
                            #endregion

                            #region else if not got item
                            else if (!gotitem)
                            {
                                com.Player.SendMessage("Transaction failed or you didn't toss item.", Color.Red);
                                TSPlayer.All.SendMessage("Transaction with " + com.Player.Name + " terminated", Color.Red);

                            } //end if not transaction
                            #endregion

                        } //end of if !null, !coin
                        #endregion
                    }

                    else if (sellValue == 0)
                        com.Player.SendMessage("Nothing selected, use /sell set to chose your first item!", Color.Red);

                    else if (sellItem.Equals(null))
                        com.Player.SendMessage("Nothing selected! use /sell set to chose your first item!", Color.Red);

                } //end of confirm
                #endregion

                else if ((com.Parameters[0].Equals("confirm") || com.Parameters[0].Equals("set")) && !com.Player.Group.HasPermission("autoshop"))
                    com.Player.SendMessage("You do not have permission to buy things!", Color.Red);

                #region default
                else if (!com.Parameters[0].Equals("confirm") && !com.Parameters[0].Equals("confirm") && !com.Parameters[0].Equals("sell"))
                {
                    com.Player.SendMessage("Available options and explanations:", Color.Yellow);
                    com.Player.SendMessage("/sell price - checks the price of the FIRST item in inventory", Color.Yellow);
                    if (com.Player.Group.HasPermission("autoshop"))
                    {
                        com.Player.SendMessage("/sell set - use FIRST - sets the first item in inventory to sell", Color.Yellow);
                        com.Player.SendMessage("/sell confirm - sells the preset (/sell set) item (on the ground)", Color.Yellow);

                    } //end autoshops

                } //end else for parameters
                #endregion

            } //end 1 parameter
            else
                com.Player.SendMessage("No parameters! try /sell help",Color.Red);

        } //end Sell

        #region Buy with /**/
        /*
        public static void Buy(CommandArgs com)
        {
            if (com.Parameters.Count == 2)
            {
                if (com.Parameters[0].Equals("price"))
                {
                    if (TShockAPI.TShock.Utils.GetItemByName(com.Parameters[1]).Count == 1)
                    {
                        //

                    } //end if one matched item
                
                } //end if price

            } //end if 2 parameter

            if (com.Parameters.Count == 1)
            {
                //

            } //end if 1 parameters

            else
            {
                //

            } //end else_1 parameter

        } //end public static Buy
        */
        #endregion

    } //end public class

} //end namespace
