# Sparta 던전 (Text RPG)

**설명**  
Sparta 던전은 .NET 6.0 기반의 콘솔 텍스트 RPG입니다. 플레이어는 캐릭터를 생성하고, 마을에서 상태 확인, 인벤토리 관리, 상점 거래, 휴식을 하며  
던전에 입장해 전투를 진행합니다. 게임 상태는 JSON 파일에 자동 저장 및 로드되어 앱을 껐다 켜도 이어서 플레이할 수 있습니다.

## 주요 기능

- **캐릭터 생성**: 이름과 직업(전사/도적) 선택
- **상태 보기**: 레벨, 체력, 골드, 장착 보너스 확인
- **인벤토리 관리**: 장비 착용/해제
- **상점**: 아이템 구매/판매
- **휴식**: 골드로 체력 회복
- **던전 입장**: 난이도별 피해 및 보상 계산
- **자동 저장/로드**: `savegame.json`에 매 턴 및 종료 시 자동 저장, 시작 시 로드
