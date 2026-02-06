namespace GameClient
{
    public enum TCPGameServerCmds
    {
        CMD_UNKNOWN = 0,

        CMD_LOGIN_ON2 = 20,
        CMD_NTF_CMD_BASE_ID = 21,
        CMD_LOG_OUT = 22,
        CMD_SPR_CLIENTHEART = 23,

        CMD_DB_QUERY_SYSPARAM = 25,
        CMD_DB_TEAMBATTLE = 26,

        #region Group Objects
        /// <summary>
        /// Thông báo có đối tượng mới xung quanh
        /// </summary>
        CMD_NEW_OBJECTS = 30,

        /// <summary>
        /// Thông báo xóa đối tượng xung quanh
        /// </summary>
        CMD_REMOVE_OBJECTS = 31,
        #endregion

        #region Pet
        /// <summary>
        /// Gói tin gửi từ GS lên GameDB truy vấn danh sách pet
        /// </summary>
        CMD_KT_DB_GET_PET_LIST = 50,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập cấp độ và kinh nghiệm cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_LEVEL_AND_EXP = 51,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập ResID cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_RESID = 52,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập lĩnh ngộ cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_ENLIGHTENMENT = 53,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập tên cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_NAME = 54,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập kỹ năng cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_SKILLS = 55,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập trang bị cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_EQUIPS = 56,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập thuộc tính pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_ATTRIBUTES = 57,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập độ vui vẻ pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_JOYFUL = 58,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập tuổi thọ pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_LIFE = 59,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập sinh lực pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_HP = 60,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB cập nhật thông tin pet trước khi thoát Game
        /// </summary>
        CMD_KT_DB_PET_UPDATE_BEFORE_QUIT_GAME = 61,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB xóa Pet
        /// </summary>
        CMD_KT_DB_DELETE_PET = 62,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thêm Pet cho người chơi tương ứng
        /// </summary>
        CMD_KT_DB_ADD_PET = 63,

        /// <summary>
        /// Gói tin gửi từ GS về Client thông báo có Pet mới
        /// </summary>
        CMD_KT_NEW_PET = 64,

        /// <summary>
        /// Gói tin gửi từ GS về Client thông báo xóa Pet
        /// </summary>
        CMD_KT_DEL_PET = 65,

        /// <summary>
        /// Gói tin gửi từ Client lên GS thông báo Pet thay đổi vị trí
        /// </summary>
        CMD_KT_PET_CHANGEPOS = 66,

        /// <summary>
        /// Gói tin yêu cầu truy vấn thông tin Pet giữa Client và GS
        /// </summary>
        CMD_KT_GET_PET_LIST = 67,

        /// <summary>
        /// Thực hiện xuất chiến hoặc thu hồi Pet giữa Client và GS
        /// </summary>
        CMD_KT_DO_PET_COMMAND = 68,

        /// <summary>
        /// Gói tin yêu cầu đổi tên pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_CHANGENAME = 69,

        /// <summary>
        /// Gói tin yêu cầu học kỹ năng pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_STUDYSKILL = 70,

        /// <summary>
        /// Gói tin thông báo pet dùng kỹ năng giữa Client và GS
        /// </summary>
        CMD_KT_PET_USE_SKILL = 71,

        /// <summary>
        /// Gói tin thông báo cập nhật các thuộc tính cơ bản của pet cho chủ nhân giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_BASE_ATTRIBUTES = 72,

        /// <summary>
        /// Gói tin thông báo cập nhật các thuộc tính sức, thân, ngoại, nội của pet cho chủ nhân giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_ATTRIBUTES = 73,

        /// <summary>
        /// Gói tin thông báo phân phối thuộc tính sức, thân, ngoại, nội của pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_ASSIGN_ATTRIBUTES = 74,

        /// <summary>
        /// Gói tin thông báo tẩy điểm tiềm năng của pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_RESET_ATTRIBUTES = 75,

        /// <summary>
        /// Gói tin yêu cầu tăng tuổi thọ hoặc độ vui vẻ cho pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_FEED = 76,

        /// <summary>
        /// Gói tin tặng quà cho pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_GIFT_ITEMS = 77,

        /// <summary>
        /// Gói tin phóng thích pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_RELEASE = 78,

        /// <summary>
        /// Gói tin cập nhật cấp độ pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_LEVEL = 79,

        /// <summary>
        /// Gói tin cập nhật danh sách pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_PETLIST = 80,

        /// <summary>
        /// Gói tin cập nhật ID chủ nhân pet vào GameDB
        /// </summary>
        CMD_KT_DB_PET_UPDATEROLEID = 81,

        /// <summary>
        /// Gói tin truy vấn thông tin pet của người chơi giữa Client và GS
        /// </summary>
        CMD_KT_PET_GET_PLAYER_LIST = 82,
        #endregion

        #region Xe tiêu
        /// <summary>
        /// Gói tin thông báo có xe tiêu mới
        /// </summary>
        CMD_KT_NEW_TRADER_CARRIAGE = 90,

        /// <summary>
        /// Gói tin thông báo xóa xe tiêu
        /// </summary>
        CMD_KT_DEL_TRADER_CARRIAGE = 91,

        /// <summary>
        /// Gói tin thông báo nhiệm vụ vận tiêu mới
        /// </summary>
        CMD_KT_NEW_CARGO_CARRIAGE_TASK = 92,

        /// <summary>
        /// Gói tin thông báo cập nhật trạng thái nhiệm vụ vận tiêu hiện tại
        /// </summary>
        CMD_KT_UPDATE_CARGO_CARRIAGE_TASK_STATE = 93,
        #endregion

        CMD_LOGIN_ON = 100,
        CMD_ROLE_LIST = 101,
        CMD_CREATE_ROLE = 102,

        CMD_INIT_GAME = 104,
        CMD_PLAY_GAME = 106,
        CMD_SPR_MOVE = 107,

        CMD_OTHER_ROLE = 110,
        CMD_OTHER_ROLE_DATA = 111,
        CMD_SPR_POSITION = 112,
        CMD_SPR_LEVEL_CHANGED = 113,

        CMD_SPR_REALIVE = 119,
        CMD_SPR_RELIFE = 120,

        /// <summary>
        /// Cập nhật thông tin các nhãn khu Obs động được mở
        /// </summary>
        CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS = 121,

        CMD_SYSTEM_MONSTER = 122,
        CMD_SPR_MAPCHANGE = 123,
        CMD_SPR_ENTERMAP = 124,
        CMD_SPR_NEWTASK = 125,

        CMD_SPR_LEAVE = 127,
        CMD_SPR_NPC_BUY = 128,
        CMD_SPR_NPC_SALEOUT = 129,
        CMD_SPR_ADD_GOODS = 130,
        CMD_SPR_MOD_GOODS = 131,
        CMD_SPR_MERGE_GOODS = 132,

        CMD_SPR_CHGCODE = 137,
        CMD_SPR_MONEYCHANGE = 138,
        CMD_SPR_MODTASK = 139,
        CMD_SPR_COMPTASK = 140,
        CMD_SPR_EXPCHANGE = 141,
        CMD_SPR_GETFRIENDS = 142,
        CMD_SPR_ADDFRIEND = 143,
        CMD_SPR_REMOVEFRIEND = 144,
        CMD_SPR_REJECTFRIEND = 145,
        CMD_SPR_ASKFRIEND = 146,
        CMD_SPR_NEWGOODSPACK = 147,
        CMD_SPR_DELGOODSPACK = 148,

        CMD_SPR_GETTHING = 150,
        CMD_SPR_CHGPKMODE = 151,
        CMD_SPR_CHGPKVAL = 152,
        CMD_SPR_UPDATENPCSTATE = 153,
        CMD_SPR_NPCSTATELIST = 154,

        CMD_SPR_ABANDONTASK = 156,

        CMD_SPR_CHAT = 159,
        CMD_SPR_USEGOODS = 160,
        CMD_SPR_CHANGEPOS = 161,
        CMD_SPR_NOTIFYCHGMAP = 162,
        CMD_SPR_FORGE = 163,

        CMD_SPR_UPDATE_ROLEDATA = 166,

        CMD_SPR_TokenCHANGE = 170,

        CMD_SPR_GOODSEXCHANGE = 172,
        CMD_SPR_EXCHANGEDATA = 173,
        CMD_SPR_MOVEGOODSDATA = 174,

        #region Sạp hàng cá nhân
        /// <summary>
        /// Thao tác gian hàng
        /// </summary>
        CMD_SPR_GOODSSTALL = 175,

        /// <summary>
        /// Thông tin gian hàng
        /// </summary>
        CMD_SPR_STALLDATA = 176,

        /// <summary>
        /// Thông báo có thằng mới mở shop
        /// </summary>
        CMD_SPR_ROLE_START_STALL = 177,

        /// <summary>
        /// Gói tin sử dụng để update đữ liệu vào game db
        /// </summary>
        CMD_SPR_STALL_UDPATE_DB = 178,

        /// <summary>
        /// Lấy toàn bộ dữ liệu mua bán từ gamedb
        /// </summary>
        CMD_SPR_STALL_QUERRY = 179,

        /// <summary>
        /// Xóa dữ liệu cửa hàng ở DB
        /// </summary>
        CMD_SPR_STALL_DEL = 180,

        /// <summary>
        /// Thông báo có thằng xóa sạp hàng
        /// </summary>
        CMD_SPR_ROLE_STOP_STALL = 181,
        #endregion

        CMD_SPR_DEAD = 183,

        CMD_SPR_QUERYIDBYNAME = 197,

        CMD_GETGOODSLISTBYSITE = 206,

        CMD_SPR_LOADALREADY = 211,

        CMD_SPR_MARKETBUYGOODS = 228,

        CMD_SPR_RESETBAG = 237,
        CMD_SPR_DAILYTASKDATA = 238,

        CMD_SPR_PORTABLEBAGDATA = 243,
        CMD_SPR_RESETPORTABLEBAG = 244,

        CMD_SPR_GETHUODONGDATA = 247,

        CMD_SPR_GETSONGLIGIFT = 252,
        CMD_SPR_CHGHUODONGID = 253,

        CMD_SPR_GETPAIHANGLIST = 271,



        CMD_SPR_CHGHALFBoundTokenPERIOD = 295,



        CMD_SPR_GETUSERMAILLIST = 365,
        CMD_SPR_GETUSERMAILDATA = 366,
        CMD_SPR_FETCHMAILGOODS = 367,
        CMD_SPR_DELETEUSERMAIL = 368,


        CMD_SPR_NEWNPC = 408,
        CMD_SPR_DELNPC = 409,

        CMD_SPR_STOPMOVE = 413,

        CMD_SPR_NOTIFYGOODSINFO = 428,

        CMD_SPR_GETUPLEVELGIFTOK = 447,



        CMD_SPR_EXECUTERECOMMENDPROPADDPOINT = 516,

        CMD_SPR_UPDATEEVERYDAYONLINEAWARDGIFTINFO = 541,
        CMD_SPR_GETEVERYDAYONLINEAWARDGIFT = 542,
        CMD_SPR_UPDATEEVERYDAYSERIESLOGININFO = 543,
        CMD_SPR_GETEVERYDAYSERIESLOGINAWARDGIFT = 544,



        CMD_SPR_GETBAITANLOG = 605,

        CMD_SPR_CHECK = 613,

        CMD_SPR_REFRESH_ICON_STATE = 616,

        CMD_SPR_SWEEP_WANMOTA = 618,

        CMD_SPR_GET_WANMOTA_DETAIL = 620,
        CMD_SPR_GET_SWEEP_REWARD = 621,

        CMD_SPR_GETLUCKYAWARD = 632,
        CMD_SPR_QUERYLUCKYAWARDINFO = 633,

        CMD_SPR_QUERYUPLEVELGIFTINFO = 634,
        CMD_SPR_GETUPLEVELGIFTAWARD = 635,

        CMD_SPR_QUERYUPLEVELGIFTINFO_ADVANCE = 640,
        CMD_SPR_GETUPLEVELGIFTAWARD_ADVANCE = 641,

        CMD_SPR_QUERYUPLEVELGIFTINFO_PREMIUM = 642,
        CMD_SPR_GETUPLEVELGIFTAWARD_PREMIUM = 643,

        CMD_SPR_BUY_UPLEVELGIFT_ADVANCE = 644,
        CMD_SPR_BUY_UPLEVELGIFT_PREMIUM = 645,

        CMD_SPR_QUERY_LIMIT_RECHAGE_INFO = 646,

        CMD_SPR_QUERY_REPAYACTIVEINFO = 637,
        CMD_SPR_GET_REPAYACTIVEAWARD = 638,
        CMD_SPR_QUERY_ALLREPAYACTIVEINFO = 639,
        CMD_SPR_GET_LIMIT_RECHAGE_AWARD = 647,

        CMD_SPR_GETUSERMAILCOUNT = 651,

        CMD_SPR_OPENMARKET2 = 654,
        CMD_SPR_MARKETSALEMONEY2 = 655,
        CMD_SPR_SALEGOODS2 = 656,
        CMD_SPR_SELFSALEGOODSLIST2 = 657,
        CMD_SPR_OTHERSALEGOODSLIST2 = 658,
        CMD_SPR_MARKETROLELIST2 = 659,
        CMD_SPR_MARKETGOODSLIST2 = 660,
        CMD_SPR_MARKETBUYGOODS2 = 661,

        CMD_SPR_UPGRADE_CHENGJIU = 672,

        CMD_SPR_PUSH_VERSION = 675,



        CMD_SPR_GET_STORE_MONEY = 762,

        CMD_SPR_STORE_MONEY_CHANGE = 764,

        CMD_SPR_WING_ZHUHUN = 811,

        CMD_SYNC_CHANGE_DAY_SERVER = 833,

        CMD_SPR_GET_YUEKA_DATA = 850,
        CMD_SPR_GET_YUEKA_AWARD = 851,

        CMD_SECOND_PASSWORD_SET = 861,



        CMD_SPR_LOGIN_WAITING_INFO = 971,

        CMD_SPR_YONGZHEZHANCHANG_JOIN = 1100,
        CMD_SPR_YONGZHEZHANCHANG_ENTER = 1101,

        CMD_SPR_YONGZHEZHANCHANG_AWARD = 1102,
        CMD_SPR_YONGZHEZHANCHANG_STATE = 1103,

        CMD_SPR_YONGZHEZHANCHANG_AWARD_GET = 1108,

        CMD_SPR_KUAFU_MAP_INFO = 1140,
        CMD_SPR_KUAFU_MAP_ENTER = 1141,

        CMD_DB_UPDATE_POS = 10001,
        CMD_DB_UPDATE_EXPLEVEL = 10002,
        CMD_DB_UPDATE_ROLE_AVARTA = 10003,
        CMD_DB_UPDATEMoney_CMD = 10004,
        CMD_DB_ADDGOODS_CMD = 10005,
        CMD_DB_UPDATEGOODS_CMD = 10006,
        CMD_DB_UPDATETASK_CMD = 10007,

        CMD_DB_UPDATEPKVAL_CMD = 10009,
        CMD_DB_UPDATEKEYS = 10010,
        CMD_DB_UPDATEToken_CMD = 10011,
        CMD_DB_UPDATEBoundToken_CMD = 10012,
        CMD_DB_MOVEGOODS_CMD = 10013,

        CMD_DB_ROLE_ONLINE = 10015,

        CMD_DB_ROLE_OFFLINE = 10017,
        CMD_DB_GET_CHATMSGLIST = 10018,

        CMD_DB_REGUSERID = 10025,

        CMD_DB_GETBANROLECATDICT = 10028,

        CMD_DB_UPDATEONLINETIME = 10032,


        CMD_DB_ADDSKILL = 10036,
        CMD_DB_UPSKILLINFO = 10037,


        CMD_DB_UPDATE_WELFARE = 10045,


        CMD_DB_GETFUBENSEQID = 10049,
        CMD_DB_UPDATEROLEDAILYDATA = 10050,
        CMD_DB_UPDATEBUFFERITEM = 10051,


        CMD_DB_UPDATECZTASKID = 10062,
        CMD_DB_GETTOTALONLINENUM = 10063,



        CMD_DB_UPDATEBANGGONG_CMD = 10071,

        CMD_DB_QUERYCHONGZHIMONEY = 10083,

        CMD_DB_SENDUSERMAIL = 10086,
        CMD_DB_GETUSERMAILDATA = 10087,

        CMD_DB_QUERYLIMITGOODSUSEDNUM = 10089,
        CMD_DB_UPDATELIMITGOODSUSEDNUM = 10090,

        CMD_DB_UPDATEUSERBoundMoney_CMD = 10095,


        CMD_DB_UPDATEROLEPARAM = 10100,



        CMD_DB_QUERYTODAYCHONGZHIMONEY = 10120,

        CMD_DB_EXECUTECHANGEOCCUPATION = 10126,

        CMD_DB_QUERY_REPAYACTIVEINFO = 10160,
        CMD_DB_GET_REPAYACTIVEAWARD = 10161,
        CMD_DB_UPDATE_ACCOUNT_ACTIVE = 10162,

        CMD_DB_GAMECONFIGDICT = 10033,


        /// <summary>
        /// Band người chơi
        /// </summary>
        CMD_DB_BAN_USER = 10026,

        /// <summary>
        /// Band người chơi theo chức năng gì đó
        /// </summary>
        CMD_DB_BAN_USER_BY_TYPE = 10027,


        CMD_DB_GAMECONIFGITEM = 10034,

        CMD_DB_SAVECONSUMELOG = 10167,
        CMD_DB_QUERYFIRSTCHONGZHIBYUSERID = 10110,

        CMD_DB_ADD_STORE_MONEY = 10174,

        CMD_DB_REQUESTNEWGMAILLIST = 10177,
        CMD_DB_MODIFYROLEGMAIL = 10178,

        CMD_DB_ROLE_BUY_YUE_KA_BUT_OFFLINE = 10181,

        CMD_DB_QUERY_ROLEMINIINFO = 10220,

        CMD_DB_GET_SERVERID = 11002,

        CMD_DB_ONLINE_SERVERHEART = 11001,


        CMD_SPR_GM_SET_MAIN_TASK = 13000,



        CMD_SPR_KF_SWITCH_SERVER = 14000,
        CMD_SPR_CHANGE_NAME = 14001,

        CMD_NTF_MAGIC_CRASH_UNITY = 14010,

        CMD_LOGDB_ADD_LOG = 20000,

        CMD_LOGDB_UPDATE_ROLE_KUAFU_DAY_LOG = 20003,

        CMD_DB_DEL_SKILL = 20101,

        CMD_SPR_TASKLIST_KEY = 30000,

        CMD_DB_ERR_RETURN = 30767,
        CMD_KT_TESTPACKET = 32123,
        /// <summary>
        /// Thiết lập vật phẩm vào khay dùng nhanh
        /// </summary>
        CMD_KT_C2G_SET_QUICK_ITEMS = 39999,
        CMD_KT_GET_NEWBIE_VILLAGES = 40000,// VỨT
        CMD_KT_G2C_RENEW_SKILLLIST = 45000,
        CMD_KT_C2G_SKILL_ADDPOINT = 45001,
        CMD_KT_C2G_SET_SKILL_TO_QUICKKEY = 45002,
        CMD_KT_C2G_SET_AND_ACTIVATE_AURA = 45003,
        CMD_KT_C2G_USESKILL = 45009,
        CMD_KT_G2C_USESKILL = 45010,
        CMD_KT_G2C_NOTIFYSKILLCOOLDOWN = 45011,
        CMD_KT_G2C_CREATEBULLET = 45012,
        CMD_KT_G2C_BULLETEXPLODE = 45013,
        CMD_KT_G2C_SKILLRESULT = 45014,
        CMD_KT_G2C_SPRITEBUFF = 45015,
        CMD_KT_G2C_BLINKTOPOSITION = 45016,
        CMD_KT_G2C_CREATEBULLETS = 45017,
        CMD_KT_G2C_BULLETEXPLODES = 45018,
        CMD_KT_G2C_MOVESPEEDCHANGED = 45019,
        CMD_KT_G2C_FLYTOPOSITION = 45020,
        CMD_KT_G2C_SPRITESERIESSTATE = 45021,
        CMD_KT_G2C_SKILLRESULTS = 45022,
        CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED = 45023,
        CMD_KT_G2C_RESETSKILLCOOLDOWN = 45024,
        CMD_KT_G2C_ATTACKSPEEDCHANGED = 45025,
        CMD_KT_GM_COMMAND = 50000,
        CMD_KT_ROLE_ATRIBUTES = 50001,
        CMD_KT_SHOW_NOTIFICATIONTIP = 50002,
        CMD_KT_FACTIONROUTE_CHANGED = 50003,
        CMD_KT_CLICKON_NPC = 50004,
        CMD_KT_G2C_NPCDIALOG = 50005,
        CMD_KT_C2G_NPCDIALOG = 50006,
        CMD_KT_G2C_CHANGEACTION = 50007,
        CMD_KT_C2G_CHANGEACTION = 50008,
        CMD_KT_G2C_SHOWDEBUGOBJECTS = 50009,
        CMD_KT_G2C_SHOWREVIVEFRAME = 50010,
        CMD_KT_C2G_CLIENTREVIVE = 50011,
        CMD_KT_SPR_NEWTRAP = 50012,
        CMD_KT_SPR_DELTRAP = 50013,
        CMD_KT_C2G_SAVESYSTEMSETTINGS = 50014,
        CMD_KT_C2G_SAVEAUTOSETTINGS = 50015,
        CMD_KT_INVITETOTEAM = 50016,
        CMD_KT_CREATETEAM = 50017,
        CMD_KT_AGREEJOINTEAM = 50018,
        CMD_KT_REFUSEJOINTEAM = 50019,
        CMD_KT_GETTEAMINFO = 50020,
        CMD_KT_KICKOUTTEAMMATE = 50021,
        CMD_KT_APPROVETEAMLEADER = 50022,
        CMD_KT_REFRESHTEAMMEMBERATTRIBUTES = 50023,
        CMD_KT_TEAMMEMBERCHANGED = 50024,
        CMD_KT_LEAVETEAM = 50025,
        CMD_KT_G2C_UPDATESPRITETEAMDATA = 50026,
        CMD_KT_ASKTOJOINTEAM = 50027,
        CMD_KT_G2C_ITEMDIALOG = 50030,
        CMD_KT_C2G_ITEMDIALOG = 50031,
        CMD_KT_C2G_OPENSHOP = 50032,
        CMD_KT_G2C_CLOSENPCITEMDIALOG = 50033,

        #region Thách đấu
        /// <summary>
        /// Gửi yêu cầu thách đấu
        /// </summary>
        CMD_KT_ASK_CHALLENGE = 50034,

        /// <summary>
        /// Phản hồi yêu cầu thách đấu
        /// </summary>
        CMD_KT_C2G_RESPONSE_CHALLENGE = 50035,

        /// <summary>
        /// Thực hiện thao tác gì đó liên quan đến thách đấu
        /// </summary>
        CMD_KT_DO_CHALLENGE_COMMAND = 50036,

        /// <summary>
        /// Nhận thông báo thông tiên thách đấu
        /// </summary>
        CMD_KT_RECEIVE_CHALLENGE_INFO = 50037,
        #endregion

        CMD_KT_C2G_AUTOPATH_CHANGEMAP = 50038,
        CMD_KT_G2C_NEW_GROWPOINT = 50039,
        CMD_KT_G2C_DEL_GROWPOINT = 50040,
        CMD_KT_C2G_GROWPOINT_CLICK = 50041,
        CMD_KT_G2C_UPDATE_PROGRESSBAR = 50042,
        CMD_KT_OPEN_TOKENSHOP = 50043,
        CMD_KT_G2C_OPEN_UI = 50044,
        CMD_KT_G2C_CLOSE_UI = 50045,
        CMD_KT_TOGGLE_HORSE_STATE = 50046,
        CMD_KT_EQUIP_ENHANCE = 50047,
        CMD_KT_COMPOSE_CRYSTALSTONES = 50048,
        CMD_KT_SPLIT_CRYSTALSTONES = 50049,
        CMD_KT_G2C_NEW_DYNAMICAREA = 50050,
        CMD_KT_G2C_DEL_DYNAMICAREA = 50051,
        CMD_KT_ASK_ACTIVEFIGHT = 50052,
        CMD_KT_G2C_START_ACTIVEFIGHT = 50053,
        CMD_KT_G2C_STOP_ACTIVEFIGHT = 50054,
        CMD_KT_CHANGE_AVARTA = 50055,
        CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT = 50056,
        CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL = 50057,
        CMD_KT_BEGIN_CRAFT = 50058,
        CMD_KT_G2C_FINISH_CRAFT = 50059,
        CMD_KT_SHOW_MESSAGEBOX = 50060,
        CMD_KT_EVENT_NOTIFICATION = 50061,
        CMD_KT_KILLSTREAK = 50062,
        CMD_KT_EVENT_STATE = 50063,
        CMD_KT_SONGJINBATTLE_RANKING = 50064,
        CMD_KT_BROWSE_PLAYER = 50065,
        CMD_KT_CHECK_PLAYER_LOCATION = 50066,
        CMD_KT_UPDATE_TITLE = 50067,
        CMD_KT_UPDATE_NAME = 50068,
        CMD_KT_UPDATE_REPUTE = 50069,
        CMD_KT_UPDATE_TOTALVALUE = 50070,
        CMD_KT_GET_BONUS_DOWNLOAD = 50071,
        CMD_KT_SEASHELL_CIRCLE = 50072,


        #region Bot
        /// <summary>
        /// Có Bot mới
        /// </summary>
        CMD_KT_G2C_NEW_BOT = 50073,
        /// <summary>
        /// Xóa bot
        /// </summary>
        CMD_KT_G2C_DEL_BOT = 50074,
        #endregion


        CMD_KT_SIGNET_ENHANCE = 50075,
        CMD_KT_QUERY_PLAYERRANKING = 50076,
        CMD_KT_C2G_CHANGE_SUBSET = 50077,
        CMD_KT_SHOW_INPUTITEMS = 50078,
        CMD_KT_SHOW_INPUTEQUIPANDMATERIALS = 50079,
        CMD_KT_FACTION_PVP_RANKING_INFO = 50081,

        CMD_KT_YOULONG = 50095,
        CMD_KT_GET_PLAYER_INFO = 50099,


        #region Bang hội
        /// <summary>
        /// Tạo bang
        /// </summary>
        CMD_KT_GUILD_CREATE = 50100,

        /// <summary>
        /// Danh sách thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_GETMEMBERLIST = 50102,

        /// <summary>
        /// Thay đổi chức vị thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGERANK = 50103,

        /// <summary>
        /// Trục xuất thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_KICK_ROLE = 50104,

        /// <summary>
        /// Cập nhật chức vị bang hội
        /// </summary>
        CMD_KT_UPDATE_GUILDRANK = 50122,

        /// <summary>
        /// Lấy ra thông tin cơ bản của bang hội
        /// Chỉ sử dụng để giao tiếp với gamedb
        /// </summary>
        CMD_KT_GUILD_GETMINI_INFO = 50125,

        /// <summary>
        /// Lấy ra thông tin bang hội
        /// </summary>
        CMD_KT_GUILD_GETINFO = 50101,

        /// <summary>
        /// Lấy thông tin tài nguyên bang hội
        /// </summary>
        CMD_KT_GUILD_GET_RESOURCE_INFO = 50126,

        /// <summary>
        /// Set thông tin tài nguyên bang hội
        /// </summary>
        CMD_KT_GUILD_SET_RESOURCE_INFO = 50127,

        /// <summary>
        /// Lấy thông tin tài nguyên nhân vật
        /// </summary>
        CMD_KT_GUILD_GET_ROLE_RESOURCE_INFO = 50128,

        /// <summary>
        /// Set tài nguyên cho nhân vật
        /// </summary>
        CMD_KT_GUILD_SET_ROLERESOURCE_INFO = 50129,

        /// <summary>
        /// Trả về danh sách xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN_LIST = 60000,

        /// <summary>
        /// Trả về danh sách kỹ năng cho bang
        /// </summary>
        CMD_KT_GUILD_SKILL_LIST = 60001,

        /// <summary>
        /// Gói tin cập nhật nhiệm vụ bang hội
        /// </summary>

        CMD_KT_GUILD_TASKUPDATE = 60002,

        /// <summary>
        /// Lấy ra thông tin nhiệm vụ
        /// </summary>
        CMD_KT_GUILD_QUEST_LIST = 60003,

        /// <summary>
        /// Danh sách các bang hội khác sử dụng để xin vào bang
        /// </summary>
        CMD_KT_GUILD_OTHER_LIST = 60004,

        /// <summary>
        /// Xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN = 60005,

        /// <summary>
        /// Duyệt hay ko duyệt thằng xin vào bang
        /// </summary>
        CMD_KT_GUILD_RESPONSE_JOINREQUEST = 60006,

        /// <summary>
        /// Cống hiến cái j đó
        /// </summary>
        CMD_KT_GUILD_DONATE = 50108,

        /// <summary>
        /// Thăng cấp bang hội
        /// </summary>
        CMD_KT_GUILD_LEVELUP = 60007,

        /// <summary>
        /// Thăng cấp kỹ năng bang hội
        /// </summary>
        CMD_KT_GUILD_SKILL_LEVELUP = 60008,

        /// <summary>
        /// Nhiệm vụ bang hội
        /// </summary>
        CMD_KT_GUILD_QUEST_CMD = 60009,

        /// <summary>
        /// Sửa công cáo bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGE_NOTIFY = 60011,

        /// <summary>
        /// Thoát khỏi bang hội
        /// </summary>
        CMD_KT_GUILD_QUIT = 60012,

        /// <summary>
        /// gói tin lấy ra thông tin công thành
        /// </summary>
        CMD_KT_GUILD_WARINFO = 60013,

        /// <summary>
        /// gói tin đăng ký công thành
        /// </summary>
        CMD_KT_GUILD_WAR_REGISTER = 60014,

        /// <summary>
        /// Gói tin mở shop
        /// </summary>
        CMD_KT_GUILD_OPENSHOP = 60015,

        /// <summary>
        /// Tuyên chiến Bang hội
        /// </summary>
        CMD_KT_GUILD_FIGHT = 61016,
        /// <summary>
        /// Đổi phe Bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGE_FACTION = 61017,

        /// <summary>
        /// 1 thằng người chơi xin vào 1 bang hội
        /// </summary>
        CMD_KT_GUILD_ASKJOIN = 50116,

        /// <summary>
        /// Thằng bagn chủ trả lời xin gia nhập
        /// </summary>
        CMD_KT_GUILD_RESPONSEASK = 50117,

        /// <summary>
        /// Mời 1 thằng vào bang hội
        /// </summary>
        CMD_KT_GUILD_INVITE = 50118,


        /// <summary>
        /// Trả lời thằng bang chủ có vào bang hay không
        /// </summary>
        CMD_KT_GUILD_RESPONSEINVITE = 50119,

        /// <summary>
        /// Thiết lập tự duyệt vào bang
        /// </summary>
        CMD_KT_GUILD_AUTO_ACCPECT_SETTING = 60010,
        #endregion


        CMD_KT_UPDATE_CURRENT_ROLETITLE = 50130,
        CMD_KT_G2C_MOD_ROLETITLE = 50131,
        CMD_KT_G2C_RECHAGE = 50132,
        CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR = 50133,
        CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP = 50134,
        CMD_KT_G2C_PLAYERPRAY = 50135,
        CMD_KT_CLIENT_SERVER_LUA = 50136,
        CMD_KT_CLIENT_DO_REFINE = 50137,
        CMD_KT_C2G_SPLIT_EQUIP_INTO_FS = 50138,
        CMD_KT_TAKEDAMAGE = 50139,
        CMD_KT_UPDATE_ROLEGUILDMONEY = 50141,
        CMD_KT_RANKING_CHECKING = 50142,
        CMD_KT_GETMARKVALUE = 50143,
        CMD_KT_UPDATEMARKVALUE = 50144,
        CMD_KT_GET_RECORE_BYTYPE = 50145,
        CMD_KT_ADD_RECORE_BYTYPE = 50146,
        CMD_KT_GETRANK_RECORE_BYTYPE = 50147,

        /// <summary>
        /// Vòng quay may mắn - đặc biệt
        /// </summary>
        CMD_KT_TURNPLATE = 50148,

        CMD_KT_GUILD_ALLTERRITORY = 50149,
        CMD_KT_UPDATE_LOCALMAP_MONSTER = 50150,
        CMD_KT_GUILD_UPDATE_TERRITORY = 50151,
        CMD_KT_GUILD_GETMINIGUILDINFO = 50152,
        CMD_KT_CAPTCHA = 50153,
        CMD_KT_LUCKYCIRCLE = 50154,

        /// <summary>
        /// Lấy thông tin của quà điểm danh
        /// </summary>
        CMD_KT_CHECKPOINT_INFO = 50155,
        /// <summary>
        /// Điểm danh ngày
        /// </summary>
        CMD_KT_CHECKPOINT_GETAWARD = 50156,


        /// <summary>
        /// Packet lấy ra top bảng xếp hạng
        /// </summary>
        CMD_KT_TOPRANKING = 50157,

        /// <summary>
        /// Packet lấy ra thông tin bảng xếp hạng
        /// </summary>
        CMD_KT_TOPRANKING_INFO = 50158,

        /// <summary>
        /// Packet nhận thưởng
        /// </summary>
        CMD_KT_TOPRANKING_GETAWARD = 50159,


        /// <summary>
        /// Packet đánh dấu vào DB là thằng này đã bú quà top rồi
        /// </summary>
        CMD_KT_UPDATE_REVICE_STATUS = 50160,

        #region Nhập mật khẩu cấp 2

        /// <summary>
        /// Thông báo nhập mật khẩu cấp 2
        /// </summary>
        CMD_KT_DO_SECONDPASSWORD_CMD = 50161,

        /// <summary>
        /// Gói tin gửi lên DB cập nhật mật khẩu cấp 2
        /// </summary>
        CMD_KT_DB_UPDATE_SECONDPASSWORD = 50162,

        #endregion Nhập mật khẩu cấp 2

        #region Tu luyện kỹ năng
        /// <summary>
        /// Gói tin truy vấn danh sách kỹ năng tu luyện
        /// </summary>
        CMD_KT_GET_EXPSKILLS = 50163,

        /// <summary>
        /// Gói tin thiết lập kỹ năng tu luyện
        /// </summary>
        CMD_KT_SET_EXPSKILL = 50164,

        /// <summary>
        /// Gói tin cập nhật thông tin kỹ năng
        /// </summary>
        CMD_KT_UPDATE_EXPSKILL = 21165,
        #endregion

        #region Phong Hỏa Liên Thành
        /// <summary>
        /// Gói tin truy vấn danh sách xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        CMD_KT_FHLC_SCOREBOARD = 50165,
        #endregion

        #region Bot bán hàng
        /// <summary>
        /// Gói tin thông báo có Bot bán hàng mới
        /// </summary>
        CMD_KT_NEW_STALLBOT = 50166,

        /// <summary>
        /// Gói tin thông báo xóa Bot bán hàng
        /// </summary>
        CMD_KT_DEL_STALLBOT = 50167,
        #endregion

        #region Mặt nạ

        CMD_KT_MASK_CHANGE_STATE = 30001,

        #endregion

        /// <summary>
        /// Gói tin sửa đồ
        /// </summary>
        CMD_KT_REPAIR_ITEM = 62000,

        /// <summary>
        /// Gửi gói tin mua Auto VIP
        /// </summary>
        CMD_KT_BUY_AUTO_VIP = 62001,

        /// <summary>
        /// Mở rộng ô chứa đồ
        /// </summary>
        CMD_KT_PORTABLEEXTRABAG = 62104,
        CMD_KT_EXTRABAG = 62105,

        CMD_KT_EFFECT_UNDER_LEG = 62106,
        CMD_KT_MAIN_TASK_JOIN_FACTION = 62107,

        /// <summary>
        /// Gói hạn giờ
        /// </summary>
        CMD_KT_LIMIT_TIME_PACKAGE_STATUS = 62200,
        CMD_KT_LIMIT_TIME_PACKAGE_INFO = 62201,
        CMD_KT_LIMIT_TIME_PACKAGE_BUY = 62202,

        /// <summary>
        /// Sự kiện pass
        /// </summary>
        CMD_DB_EVENT_PASS_UPDATE = 62203, //Only Server Need
        CMD_DB_EVENT_PASS_GET = 62204, //Only Server Need
        CMD_KT_EVENT_PASS_LIST = 62205,
        CMD_KT_EVENT_PASS_INFO = 62206,
        CMD_KT_EVENT_PASS_BUY_VIP = 62207,
        CMD_KT_EVENT_PASS_RECEIVE = 62208,
        CMD_KT_EVENT_PASS_BUY_POINT = 62209,
        CMD_DB_EVENT_PASS_CREATE = 62210, //Only Server Need
        CMD_KT_EVENT_PASS_UPDATE_NOTIFY = 62211,

        /// <summary>
        /// Sự kiện newbie package
        /// </summary>
        CMD_DB_NEWBIE_PACKAGE_CREATE = 62212, //Only Server Need
        CMD_DB_NEWBIE_PACKAGE_UPDATE = 62213, //Only Server Need
        CMD_DB_NEWBIE_PACKAGE_GET = 62214, //Only Server Need
        CMD_DB_NEWBIE_PACKAGE_GET_LIST_ACTIVED = 62215, //Only Server Need
        CMD_KT_NEWBIE_PACKAGE_STATUS = 62216,
        CMD_KT_NEWBIE_PACKAGE_INFO = 62217,
        CMD_KT_NEWBIE_PACKAGE_BUY = 62218,

        /// <summary>
        /// Sự kiện Pass Mới
        /// </summary>
        CMD_DB_EVENT_PASS_NEW_UPDATE = 63002,
        CMD_DB_EVENT_PASS_NEW_GET = 63003,
        CMD_KT_EVENT_PASS_NEW_LIST = 63004,
        CMD_KT_EVENT_PASS_NEW_INFO = 63005,
        CMD_KT_EVENT_PASS_NEW_BUY_VIP = 63006,
        CMD_KT_EVENT_PASS_NEW_RECEIVE = 63007,
        CMD_KT_EVENT_PASS_NEW_BUY_POINT = 63008,
        CMD_DB_EVENT_PASS_NEW_CREATE = 63009,
        CMD_KT_EVENT_PASS_NEW_UPDATE_NOTIFY = 63010,

        /// <summary>
        /// Check nạp qua web
        /// </summary>
        CMD_KT_FIREBASE_TRACKING_INAPP_PURCHASE = 63011,

        CMD_KT_TUTORIAL_CACHE_STATE = 62300,
        CMD_KT_TUTORIAL_GET_CACHE_STATE = 62301,

        CMD_SPR_ADD_LIST_GOODS = 63000,
        CMD_KT_CLIENT_NOTI_IAP = 63001,
        CMD_SPR_ADD_LIST_AWARD = 63002,

        CMD_KT_OPEN_INAPP_SHOP = 64001,
        CMD_KT_GET_PACKAGE_INAPP_SHOP = 64002,

        CMD_KT_VERIFY_ACCOUNT_INFO = 65000,
        CMD_KT_UPDATE_STATUS_VERIFY_ACCOUNT = 65001,
        CMD_KT_VERIFY_ACCOUNT_RECEIVE_REWARD = 65002,
        CMD_KT_VERIFY_ACCOUNT_GET_STATUS = 65003,
        CMD_KT_VERIFY_BUTTON_VERIFY = 65004,

        //Sự kiện nhiệm vụ trưởng thành
        CMD_DB_MATURE_QUEST_CREATE = 65006,
        CMD_DB_MATURE_QUEST_UPDATE = 65007,
        CMD_DB_MATURE_QUEST_UPDATE_STATE = 65008,
        CMD_DB_MATURE_QUEST_DETAIL_CREATE = 65009,
        CMD_DB_MATURE_QUEST_DETAIL_UPDATE = 65010,
        CMD_DB_MATURE_QUEST_GET_CURRENT_INFO = 65011,
        CMD_DB_MATURE_QUEST_GET = 65012,
        CMD_KT_MATURE_QUEST_STATUS = 65013,
        CMD_KT_MATURE_QUEST_INFO = 65014,
        CMD_KT_MATURE_QUEST_REWARD = 65015,
        CMD_KT_MATURE_QUEST_EVENT_ACTIVE = 65016,
        CMD_KT_MATURE_QUEST_EVENT_BUY_VIP = 65017,

        //Event preview
        CMD_KT_EVENT_LIST_PREVIEW_STATUS = 65018,
        CMD_KT_EVENT_LIST_PREVIEW_GO = 65019,

        /// <summary>
        /// Gói hạn giờ
        /// </summary>
        CMD_KT_LIMIT_TIME_PACKAGE_NEW_STATUS = 65020,
        CMD_KT_LIMIT_TIME_PACKAGE_NEW_INFO = 65021,
        CMD_KT_LIMIT_TIME_PACKAGE_NEW_BUY = 65022,
        CMD_DB_UPDATE_TIME_PACKAGE = 65023,
        CMD_DB_GET_TIME_PACKAGE_INFO = 65024,
        CMD_DB_GET_TIME_PACKAGE_ALL = 65025,

        /// <summary>
        /// Mở khóa tính năng mới
        /// </summary>
        CMD_KT_UNLOCK_FEATURE = 65026,
        CMD_KT_FIRST_PURCHASE_INFO = 65027,

        CMD_SPR_NEW_TASK_MATURE_QUEST = 65029,
        CMD_SPR_UPDATE_TASK_MATURE_QUEST = 65030,

        CMD_KT_GET_LOCAL_NOTIFI = 65031,

        /// <summary>
        /// Gói mua gia hạn tự động
        /// </summary>
        CMD_KT_GET_ALL_RENEWAL_PACKAGE_INFO = 65032,
        CMD_KT_GET_RENEWAL_PACKAGE_STATUS = 65033,
        CMD_KT_RENEWAL_PACKAGE_BUY = 65034,
        CMD_KT_RENEWAL_PACKAGE_CANCEL_BUY = 65035,
        CMD_KT_RENEWAL_PACKAGE_CHECK_SLOT = 65039,

        /// <summary>
        /// Assasin Boss Rank
        /// </summary>
        CMD_KT_GET_ASSASIN_BOSS_RANK = 62501,

        /// <summary>
        /// Đổi giơi tính
        /// </summary>
        CMD_SPR_CHANGE_SEX = 62506,

        /// <summary>
        /// Đăng xuất
        /// </summary>
        CMD_KT_DO_LOG_OUT,
        /// <summary>
        /// Trùng sinh
        /// </summary>
        CMD_REINCARNATION,
        /// <summary>
        /// Cập nhật trùng sinh nhân vật
        /// </summary>
        CMD_UPDATE_REINCARNATION,

        /// <summary>
        /// Trạng thái đăng nhập và nhạn quà của sự kiện ng chơi cũ trở lại
        /// </summary>
        CMD_KT_COMBACK_LOGIN_7_DAY_STATUS = 62510,
        /// <summary>
        /// Thực hiện nhận quà sự kiện người chơi cũ trở lại
        /// </summary>
        CMD_KT_COMBACK_LOGIN_7_DAY_RECEIVE = 62511,
        /// <summary>
        /// Hiển thị button sự kiện ở client
        /// </summary>
        CMD_KT_COMBACK_LOGIN_7_DAY_BUTTON = 62512,

        /// <summary>
        /// Danh sách thời gian của activity
        /// </summary>
        CMD_KT_ACITIVITY_LIST_TIME = 62305,

        /// <summary>
        /// Tỉ lệ bonus thêm khi cường hoá trang bị
        /// </summary>
        CMD_KT_ITEM_REFINE_BONUS_PER_INFO = 62306,
    };
}
