public enum LocalizationKey
{
    Action_Right                                       , // 이동(우)
    Action_Left                                        , // 이동(좌)
    Action_DownUtility                                 , // 하강/앉기/드랍
    Action_UpUtility                                   , // 상승 및 투척
    Action_Dash                                        , // 대시
    Action_Jump                                        , // 점프
    Action_Attack                                      , // 공격
    Action_Interact                                    , // 상호 작용
    Action_QuickSlot1                                  , // 퀵슬롯1
    Action_QuickSlot2                                  , // 퀵슬롯2
    Action_QuickSlot3                                  , // 퀵슬롯3
    Action_QuickSlot4                                  , // 퀵슬롯4
    Action_Inventory                                   , // 인벤토리
    Action_Active                                      , // 액티브 스킬
    Action_Pause                                       , // 일시정지 메뉴
    Action_Cancel                                      , // 팝업 캔슬
    Action_DashCommand                                 , // 대시 커맨드
    None                                               , // None
    Apply                                              , // 적용
    Complete                                           , // 완료
    Cancel                                             , // 취소
    Default                                            , // 기본값
    Language                                           , // 언어
    Reset                                              , // 초기화
    Switch                                             , // 전환
    Play                                               , // 플레이
    Scene_Bootstrap                                    , // 진입점
    Scene_Hospital1                                    , // 병원 입구 1
    Scene_Hospital2                                    , // 병원 입구 2
    Scene_Hospital3                                    , // 병원 입구 3
    Scene_Counter                                      , // 카운터
    Scene_ConvenienceStore                             , // 매점
    Scene_ElevatorHallway                              , // 엘리베이터 복도
    Scene_Lobby                                        , // 로비
    Scene_Corridor                                     , // 복도
    Scene_Kitchen                                      , // 주방
    Scene_Terrace                                      , // 테라스
    Scene_GroceryStorage                               , // 식료품 창고
    Scene_EmployeeBreakroom                            , // 직원 휴게실
    Scene_Cafeteria                                    , // 구내 식당
    Scene_Office                                       , // 사무실
    Scene_PretreatmentRoom                             , // 전처리실
    Scene_HumanStorage                                 , // 인간 창고
    Slot_Auto                                          , // Auto
    Slot_Day_Format                                    , // Day {0}
    Slot_DayTime_Format                                , // {0}:{1}:{2}
    Slot_SaveTime_Format                               , // {0}/{1}/{2}
    Console_Parameter_Format                           , // - {0}
    Console_Input_Format                               , // > {0}
    Console_Help_Header                                , // --- 사용 가능한 명령어 목록 ---
    Console_Help_Format                                , // - {0}: {1}
    Console_Help_Detail                                , // [{0}] {1}
    Console_Help_NotFound                              , // {0}' 명령어에 대한 설명을 찾을 수 없습니다.
    Console_Desc_Help                                  , // 사용 가능한 명령어 목록을 표시하거나 명령어의 상세 설명을 확인합니다.
    Console_Desc_Debug                                 , // 디버그 모드를 켜거나 끕니다.
    Console_Desc_Clear                                 , // 콘솔창의 로그 기록을 지웁니다.
    Console_Desc_FPS                                   , // FPS 표시 UI를 켜거나 끕니다.
    Console_Desc_Time                                  , // 게임 타임 스케일을 설정하거나 확인합니다.
    Console_Desc_Set                                   , // 임의의 현재 변수를 설정합니다.
    Console_Desc_SetBase                               , // 임의의 기준 변수를 설정합니다.
    Console_Desc_Get                                   , // 지정한 변수의 값을 가져옵니다.
    Console_Desc_LogSearch                             , // 로그 메시지에서 특정 키워드를 검색합니다.
    Console_Desc_LogFilter                             , // 특정 타입의 로그 가시성을 설정합니다.
    Console_Desc_Ground                                , // 땅 감지 박스 디버그 표시를 토글합니다.
    Console_Desc_Scene                                 , // 지정한 맵으로 즉시 이동합니다.
    Console_Desc_Spawn                                 , // 지정한 캐릭터를 Demo 맵에 소환합니다.
    Console_Desc_Item                                  , // 지정한 아이템을 생성, 제거하거나 인벤토리를 비웁니다.
    Console_Desc_Save                                  , // 현재 게임 상태를 저장합니다.
    Console_Debug_Toggle                               , // 디버그 모드: {0}
    Console_Clear_Success                              , // 콘솔 로그를 초기화했습니다.
    Console_Clear_NotOpen                              , // 콘솔 UI가 열려있지 않습니다.
    Console_FPS_Disabled                               , // FPS UI를 껐습니다.
    Console_FPS_Enabled                                , // FPS UI를 켰습니다.
    Console_Time_Set                                   , // 타임 스케일이 {0}(으)로 설정되었습니다.
    Console_Time_Current                               , // 현재 타임 스케일: {0}
    Console_Variable_Header                            , // --- 사용 가능한 능력치 목록 ---
    Console_Set_Usage                                  , // 사용법: set [이름] [값]
    Console_Set_Success                                , // 변수 설정 완료 [{0} = {1}]
    Console_SetBase_Usage                              , // 사용법: setbase [이름] [값]
    Console_SetBase_Success                            , // 변수 설정 완료 [{0} = {1}]
    Console_Get_Usage                                  , // 사용법: get [이름]
    Console_Get_Success                                , // [{0}] = {1}
    Console_Get_NotFound                               , // {0}' 변수를 찾을 수 없습니다.
    Console_LogSearch_Reset                            , // 로그 검색 필터를 초기화했습니다.
    Console_LogSearch_Filtered                         , // 로그 검색 키워드: '{0}'
    Console_LogFilter_Usage                            , // 사용법: log_filter [info/warn/error/system] [true/false]
    Console_LogFilter_InvalidBool                      , // 올바른 불리언(true/false) 값이 아닙니다.
    Console_LogFilter_UnknownType                      , // 알 수 없는 로그 필터 타입입니다: '{0}'
    Console_LogFilter_Success                          , // 로그 필터 변경 [{0} -> {1}]
    Console_Ground_Toggle                              , // 땅 감지 디버그 박스 표시: {0}
    Console_Scene_Usage                                , // 사용법: scene [SceneID] (사용 가능한 맵 목록 'scene list' 입력)
    Console_Scene_Available                            , // --- 이동 가능한 맵 목록 ---
    Console_Scene_MovingProcess                        , // {0}' 맵으로 이동 중...
    Console_Scene_Invalid                              , // 유효하지 않은 SceneID입니다: '{0}'. 맵 목록을 보려면 'scene list'를 입력하세요.
    Console_Spawn_Available                            , // --- 소환 가능한 캐릭터 목록 ---
    Console_Spawn_Usage                                , // 사용법: spawn [CharacterID]
    Console_Spawn_NotDemo                              , // 이 명령어는 Demo 맵에서만 사용할 수 있습니다.
    Console_Spawn_NotFoundPlayableCharacter            , // 활성화된 플레이어 캐릭터가 없어 기본 '{0}' 캐릭터를 먼저 생성합니다.
    Console_Spawn_Success                              , // 캐릭터 [{0}] 소환 완료!
    Console_Spawn_Invalid                              , // 유효하지 않은 CharacterID입니다: '{0}'. 캐릭터 목록을 보려면 'spawn list'를 입력하세요.
    Console_Item_Available                             , // --- 소환 가능한 아이템 목록 ---
    Console_Item_Usage                                 , // 사용법: item [ItemID] [Quantity]
    Console_Item_InvalidID                             , // 유효하지 않은 ItemID입니다: '{0}'.
    Console_Item_InventoryFull                         , // 인벤토리가 가득 차서 아이템을 소환할 수 없습니다.
    Console_Item_Success                               , // [{0}] {1} 아이템을 {2}개 소환했습니다.
    Console_Item_RemoveUsage                           , // 사용법: item remove [ItemID] [Quantity]
    Console_Item_RemoveSuccess                         , // [{0}] 아이템 {1}개 제거했습니다.
    Console_Item_RemoveFailed                          , // 인벤토리에 제거할 아이템([{0}])이 없거나 잘못되었습니다.
    Console_Item_ClearSuccess                          , // 인벤토리를 비웠습니다.
    Console_Save_DefaultSlotSelected                   , // 기본 슬롯({0}번)에 저장합니다.
    Console_Save_Success                               , // 게임 저장을 완료했습니다.
    Console_Save_InvalidSlot                           , // 유효하지 않은 세이브 슬롯 번호입니다: '{0}'.
    Interaction_Ladder                                 , // 사다리 진입
    Interaction_Tray                                   , // 식판 획득/내려놓기/던지기/식탁 배치
    Item_Name_1                                        , // 대검
    Item_Description_1                                 , // 공격력: 1\n공격속도: 1
    Item_Flavor_1                                      , // 테스트용 대검이다.
    Log_Attribute_Registry_Unsupported                 , // [AttributeRegistry] 지원하지 않는 데이터 타입입니다: {0}('{1}')
    Log_Camera_LoadSuccess                             , // [CameraManager] 카메라 로드에 성공했습니다.
    Log_Camera_LoadFailed                              , // [CameraManager] 카메라 로드에 실패했습니다.
    Log_Config_CreatedNew                              , // [ConfigManager] 새로운 설정 파일을 생성했습니다.
    Log_Config_LoadedSuccessfully                      , // [ConfigManager] 설정 파일을 성공적으로 불러왔습니다.
    Log_Config_LoadFailed                              , // [ConfigManager] 설정 파일을 불러오는 중 오류가 발생했습니다.
    Log_Config_SaveFailed                              , // [ConfigManager] 설정 파일을 저장하는 중 오류가 발생했습니다.
    Log_Config_Reset                                   , // [ConfigManager] 설정을 초기화했습니다.
    Log_Config_ConsoleEnabled                          , // [ConfigManager] 명령어 인자에 의해 디버그 콘솔이 활성화되었습니다.
    Log_Config_DebugEnabled                            , // [ConfigManager] 명령어 인자에 의해 디버그 모드가 활성화되었습니다.
    Log_Console_NoDescription                          , // [ConsoleManager] 제공된 설명이 없습니다.
    Log_Console_Error                                  , // [ConsoleManager] 명령어 실행 중 오류가 발생했습니다 ('{0}')
    Log_Console_UnknownCommand                         , // [ConsoleManager] 알 수 없는 명령어입니다: '{0}'. 사용 가능한 명령어 목록을 보려면 'help'를 입력하세요.
    Log_Control_AssetLoadFailed                        , // [ControlManager] 인풋 액션 자산을 불러오지 못했습니다: {0}
    Log_Control_LoadedSuccessfully                     , // [ControlManager] 인풋 제어 시스템을 성공적으로 불러왔습니다.
    Log_Control_MapNotFound                            , // [ControlManager] 활성화할 액션 맵을 찾을 수 없습니다: '{0}'
    Log_Data_LoadedSuccessfully                        , // [DataManager] 데이터 테이블 '{0}'을(를) 성공적으로 불러왔습니다.
    Log_Data_AssetNotFound                             , // [DataManager] 데이터 에셋을 찾을 수 없습니다: '{0}'
    Log_Data_DeserializeFailed                         , // [DataManager] 테이블 '{0}'의 역직렬화 중 오류가 발생했습니다.
    Log_Data_DuplicateKey                              , // [DataManager] 테이블 '{0}'에서 중복된 키가 발견되어 무시됩니다: '{1}'
    Log_Feedback_AlertPopup_Cancelled                  , // [FeedbackManager] 알림 팝업 대기가 취소되었습니다.
    Log_Feedback_ConfirmPopup_Cancelled                , // [FeedbackManager] 확인 팝업 대기가 취소되었습니다.
    Log_Game_Loading_DebugData                         , // [GameManager] 디버그 전용 게임 데이터를 생성하는 중입니다.
    Log_Game_Loading_NewData                           , // [GameManager] 세이브 파일을 불러오는 중입니다.
    Log_Game_Loading_SaveData                          , // [GameManager] 새로운 게임 데이터를 생성하는 중입니다.
    Log_Game_Loading_PlayerSpawn                       , // [GameManager] 캐릭터를 생성하고 월드로 이동하는 중입니다.
    Log_Game_Loading_ResourcePackaging                 , // [GameManager] 리소스를 불러오는 중입니다.
    Log_Game_Loading_Title                             , // [GameManager] 타이틀 화면으로 이동합니다.
    Log_Game_CharacterSpawnFailed                      , // [GameManager] 캐릭터('{0}') 생성에 실패했습니다.
    Log_Game_CharacterSpawnSuccess                     , // [GameManager] 캐릭터('{0}')를 성공적으로 생성했습니다.
    Log_Graphic_RootInitialized                        , // [UIManager] 그래픽 볼륨 구조가 성공적으로 초기화되었습니다.
    Log_Interact_NotRegistered                         , // [InteractManager] 등록되지 않은 상호작용 타입입니다: '{0}'
    Log_Localization_LoadedSuccessfully                , // [LocalizationManager] 현지화 시스템을 성공적으로 불러왔습니다.
    Log_Localization_FileReadFailed                    , // [LocalizationManager] 현지화 파일('{0}')을 읽는 중 오류가 발생했습니다.
    Log_Localization_Synced                            , // [LocalizationManager] 현지화 데이터 동기화를 완료했습니다: (변경: '{0}'건)
    Log_Localization_SyncFailed                        , // [LocalizationManager] 현지화 파일 동기화 중 오류가 발생했습니다.
    Log_Localization_SaveFailed                        , // [LocalizationManager] 현지화 파일('{0}') 저장에 실패했습니다.
    Log_Localization_LanguageFileParseFailed           , // [LocalizationManager] 언어 지원 파일('{0}')을 분석하는 중 오류가 발생했습니다.
    Log_Log_SetupCompleted                             , // [LogManager] 로깅 시스템 초기화 및 대기열 처리를 완료했습니다.
    Log_Pool_InstantiateFailed                         , // [PoolManager] 키에 해당하는 오브젝트 생성에 실패했습니다: '{0}'
    Log_Pool_DestroyResult                             , // [PoolManager] {0}개의 풀링 오브젝트를 파괴했습니다: (Key: '{1}')
    Log_Pool_Cleared                                   , // [PoolManager] 오브젝트 풀을 정리했습니다. 총 파괴된 개수: {0}
    Log_Preload_BootStarted                            , // [PreloadManager] 게임 초기 부팅 프리로드 프로세스를 시작합니다.
    Log_Preload_Boot_Data                              , // [PreloadManager] 환경 설정을 불러오는 중...
    Log_Preload_Boot_Asset                             , // [PreloadManager] 에셋 리소스를 불러오는 중...
    Log_Preload_Boot_UI                                , // [PreloadManager] UI 리소스를 불러오는 중...
    Log_Preload_Boot_Object                            , // [PreloadManager] 오브젝트 풀을 불러오는 중...
    Log_Preload_BootFinished                           , // [PreloadManager] 게임 초기 부팅 프리로드 프로세스가 성공적으로 완료되었습니다.
    Log_Resource_LoadFailed_Null                       , // [ResourceManager] 경로('{0}')의 에셋을 불러왔으나 결과가 null 입니다.
    Log_Resource_LoadFailed_Exception                  , // [ResourceManager] 경로('{0}')의 에셋을 로드하는 중 예외가 발생했습니다.
    Log_Save_RestoredFromBackup                        , // [SaveManager] 슬롯 {0}: 기본 파일이 없어 백업 파일에서 복원했습니다.
    Log_Save_LoadSuccess                               , // [SaveManager] 슬롯 {0} 데이터를 성공적으로 불러왔습니다.
    Log_Save_LoadFailed                                , // [SaveManager] 슬롯 {0} 데이터 로드 중 예외가 발생하여 새 게임을 시작합니다.
    Log_Save_SaveSuccess                               , // [SaveManager] 슬롯 {0} 데이터 저장을 완료했습니다.
    Log_Save_SaveFailed                                , // [SaveManager] 슬롯 {0} 데이터 저장 중 예외가 발생했습니다.
    Log_Save_MetaLoadFailed                            , // [SaveManager] 메타데이터 로드 중 예외가 발생했습니다.
    Log_Save_MetaSaveFailed                            , // [SaveManager] 메타데이터 저장 중 예외가 발생했습니다.
    Log_Save_ClearSuccess                              , // [SaveManager] 슬롯 {0}을 삭제했습니다.
    Log_Save_NewGameStarted                            , // [SaveManager] 슬롯 {0}에서 새 게임을 생성했습니다.
    Log_Scene_LoadSuccess                              , // [SceneManager] '{0}'맵을 성공적으로 로드했습니다.
    Log_Scene_LoadFailed                               , // [SceneManager] '{0}'맵을 로드하는 중 에셋을 찾지 못했습니다.
    Log_Scene_TransitionFailed                         , // [SceneManager] '{0}'맵에서 '{1}'맵으로의 씬 전환 규칙이 존재하지 않습니다.
    Log_Scene_NotFoundCharacter                        , // [SceneManager] 캐릭터가 존재하지 않아 스폰 위치로 이동할 수 없습니다.
    Log_Scene_NotExistPreviousScene                    , // [SceneManager] 이전 맵이 존재하지 않습니다.
    Log_Scene_NormalizedSpawn                          , // [SceneManager] 캐릭터를 스폰 포인트로 보정하여 이동시켰습니다: '{0}'
    Log_Scene_NotFoundSpawnpoint                       , // [SceneManager] '{0}'에 해당하는 스폰 포인트를 찾지 못했습니다.
    Log_UI_RootInitialized                             , // [UIManager] UI 루트 및 레이어 구조가 성공적으로 초기화되었습니다.
    Log_UI_OpenDisplayFailed                           , // [UIManager] 디스플레이 UI({0})를 생성(Pop)하는 데 실패했습니다.
    Log_UI_OpenPopupFailed                             , // [UIManager] 팝업 UI({0})를 생성(Pop)하는 데 실패했습니다.
    Log_UI_OpenSystemFailed                            , // [UIManager] 시스템 UI({0})를 생성(Pop)하는 데 실패했습니다.
    Log_UI_NotFoundControlManager                      , // [UIManager] 컨트롤 매니저를 참조할 수 없습니다.
    Log_Console_System_ProcessFailed                   , // [UIConsoleSystem] 명령어('{0}') 처리 중 예외가 발생했습니다.
    Log_Keybind_Slot_RebindFailed                      , // [UIKeybindSlot] 액션('{0}') 키 리바인딩 시작 중 예외가 발생했습니다.
    Log_Save_Slot_SlotClickFailed                      , // [UISaveSlot] 슬롯({0}) 클릭 처리 중 예외가 발생했습니다.
    Log_Save_Slot_MoveUpFailed                         , // [UISaveSlot] 슬롯({0}) 위로 이동 중 예외가 발생했습니다.
    Log_Save_Slot_MoveDownFailed                       , // [UISaveSlot] 슬롯({0}) 아래로 이동 중 예외가 발생했습니다.
    Log_Option_Popup_ApplyFailed                       , // [UIOptionPopup] 옵션 적용 중 예외가 발생했습니다.
    Log_Option_Popup_CompleteFailed                    , // [UIOptionPopup] 옵션 완료 저장 중 예외가 발생했습니다.
    Log_Option_Popup_DefaultFailed                     , // [UIOptionPopup] 옵션 초기화 중 예외가 발생했습니다.
    Log_Option_Popup_Keybind_Duplicate                 , // [UIOptionPopup] 이미 '{0}'에 '{1}'키가 지정되어 있습니다.
    Log_Load_Display_AnimationFailed                   , // [UILoadDisplay] 로딩 애니메이션 진행 중 예외가 발생했습니다.
    Log_Load_Display_RotateFailed                      , // [UILoadDisplay] 로딩 회전 애니메이션 중 예외가 발생했습니다.
    Log_Splash_Display_Skip                            , // [UISplashScreen] 스플래시 이미지를 건너뛰었습니다.
    Shop_Name_1                                        , // 무기 상점
    Shop_Description_1                                 , // 무기 상점이다.
    UI_Console_System_AutoComplete_Candidates          , // 자동 완성 후보: {0}
    UI_FPS_System_Indicator                            , // FPS: {0}
    UI_Pause_Popup_Text_Continue                       , // 계속하기
    UI_Pause_Popup_Text_Option                         , // 설정
    UI_Pause_Popup_Text_Title                          , // 타이틀
    UI_Pause_Popup_Confirm_Title                       , // 경고
    UI_Pause_Popup_Confirm_Desc                        , // 저장되지 않은 사항을 잃어버릴 수 있습니다. \n계속하시겠습니까?
    UI_Title_Display_Text_Press_Any_Key                , // Press any key to start.
    UI_Option_Popup_Text_Sound                         , // 사운드
    UI_Option_Popup_Text_Graphic                       , // 그래픽
    UI_Option_Popup_Text_Access                        , // 접근성
    UI_Option_Popup_Text_Master                        , // 마스터 볼륨
    UI_Option_Popup_Text_BGM                           , // 음악
    UI_Option_Popup_Text_Ambient                       , // 환경음
    UI_Option_Popup_Text_SFX                           , // 효과음
    UI_Option_Popup_Text_UI                            , // UI 볼륨
    UI_Option_Popup_Text_Mute                          , // 프로그램 백그라운드 시 음소거
    UI_Option_Popup_Text_Resolution                    , // 해상도
    UI_Option_Popup_Text_Resolution_Dropdown           , // {0} x {1} ({2}hz)
    UI_Option_Popup_Text_Fullscreen_Windowed           , // 창모드
    UI_Option_Popup_Text_Fullscreen_FullscreenWindow   , // 테두리 없는 창모드
    UI_Option_Popup_Text_Fullscreen_ExclusiveFullscreen, // 전체화면
    UI_Option_Popup_Text_Quality                       , // 텍스쳐
    UI_Option_Popup_Text_Quality_High                  , // 높음
    UI_Option_Popup_Text_Quality_Medium                , // 중간
    UI_Option_Popup_Text_Quality_Low                   , // 낮음
    UI_Option_Popup_Text_Vsync                         , // 수직 동기화
    UI_Option_Popup_Text_Antialiasing                  , // 안티 앨리어싱
    UI_Option_Popup_Text_Bloom                         , // 블룸
    UI_Option_Popup_Text_Vignette                      , // 비네트
    UI_Option_Popup_Text_MotionBlur                    , // 모션 블러
    UI_Option_Popup_Text_Contrast                      , // 대조비
    UI_Option_Popup_Text_Keybind                       , // 키보드 조작 설정
    UI_Option_Popup_Text_Bind                          , // >설정할 키 입력<
    UI_Option_Popup_Text_Modifier                      , // 대시키+방향키
    UI_Option_Popup_Text_Tap                           , // 방향키 더블탭
    UI_Option_Popup_Default_Confirm_Title              , // 기본값 초기화
    UI_Option_Popup_Default_Confirm_Message            , // 모든 설정을 기본값으로 되돌리시겠습니까?
    UI_SaveDetail_Popup_Delete_Confirm_Title           , // 세이브 삭제
    UI_SaveDetail_Popup_Delete_Confirm_Message         , // 정말 이 세이브를 삭제하시겠습니까?
}
