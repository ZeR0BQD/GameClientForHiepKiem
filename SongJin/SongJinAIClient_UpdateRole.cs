using GameClient.Data;
using HSGameEngine.GameEngine.Network.Protocol;
using MySqlX.XDevAPI.Common;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.SongJin
{
    public partial class SongJinAIClient
    {
        private void CMD_SPR_UPDATE_ROLEDATA(SpriteLifeChangeData data)
        {
            if (RoleData != null && RoleData.RoleID == data.RoleID)
            {
                RoleData.CurrentHP = data.HP;
                RoleData.CurrentMP = data.MP;
                RoleData.MaxHP = data.MaxHP;
                RoleData.MaxMP = data.MaxMP;
                RoleData.MaxStamina = data.MaxStamina;
                RoleData.CurrentStamina = data.Stamina;

                if (data.HP <= 0)
                {
                    Send_CMD_KT_C2G_CLIENTREVIVE();
                }
                if (data.HP > 0)
                {
                    CLIENTREVIVE = false;
                }
            }
        }

        private void CMD_SPR_CHANGEPOS(params string[] args)
        {
            var roleId = Convert.ToInt32(args[0]);
            if (RoleData != null && roleId == RoleData.RoleID)
            {
                RoleData.PosX = Convert.ToInt32(args[1]);
                RoleData.PosY = Convert.ToInt32(args[2]);

                Console.WriteLine("Pos: {0} {1}", RoleData.PosX, RoleData.PosY);
            }
        }

        private void CMD_SPR_NOTIFYCHGMAP(params string[] args)
        {
            Thread.Sleep(1000);
            if (RoleData == null)
            {
                return;
            }

            int teleportID = Convert.ToInt32(args[0]);
            int toMapCode = Convert.ToInt32(args[1]);
            int toMapX = Convert.ToInt32(args[2]);
            int toMapY = Convert.ToInt32(args[3]);
            SCMapChange mapChangeData = new SCMapChange
            {
                RoleID = RoleID,
                TeleportID = -1,
                MapCode = toMapCode,
                PosX = toMapX,
                PosY = toMapY,
            };

            RoleData.MapCode = mapChangeData.MapCode;
            RoleData.PosX = toMapX;
            RoleData.PosY = toMapY;

            byte[] bData = DataHelper.ObjectToBytes<SCMapChange>(mapChangeData);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bData, 0, bData.Length, (int)(TCPGameServerCmds.CMD_SPR_MAPCHANGE)));
        }

        private void CMD_KT_G2C_ITEMDIALOG(G2C_LuaItemDialog result)
        {
            Thread.Sleep(1000);
            var selection = result.Selections.FirstOrDefault(s => s.Value == "Thành thị");
            if (selection.Value != null)
            {
                CMD_KT_C2G_ITEMDIALOG(result, selection.Key);
                return;
            }
            selection = result.Selections.FirstOrDefault(s => s.Value == "Phượng Tường");
            if (selection.Value != null)
            {
                CMD_KT_C2G_ITEMDIALOG(result, selection.Key);
            }

        }

        private void CMD_KT_C2G_ITEMDIALOG(G2C_LuaItemDialog result, int selection)
        {
            C2G_LuaItemDialog data = new C2G_LuaItemDialog()
            {
                ID = result.ID,
                ItemID = result.ItemID,
                DbID = result.DbID,
                SelectionID = selection,
                SelectedItem = null,
                OtherParams = result.OtherParams,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_LuaItemDialog>(data);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_ITEMDIALOG)));
        }

        private void Send_CMD_KT_CLICKON_NPC(int npcId)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(npcId.ToString());
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_ITEMDIALOG)));
        }

        bool CLIENTREVIVE = false;
        private void Send_CMD_KT_C2G_CLIENTREVIVE()
        {
            if (CLIENTREVIVE)
            {
                return;
            }
            CLIENTREVIVE = true;
            Thread.Sleep(1000);

            C2G_ClientRevive data = new C2G_ClientRevive()
            {
                SelectedID = 1,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_ClientRevive>(data);
            loginClient.SendData(TCPOutPacket.MakeTCPOutPacket(loginClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_CLIENTREVIVE)));
        }
    }
}
