# loveSimulation - Unity 2D 연애 시뮬레이션 개발 룰

## 프로젝트 개요
- **엔진**: Unity 6 (6000.3.6f1)
- **렌더링**: Universal Render Pipeline (URP) 2D
- **입력**: Unity New Input System
- **해상도**: 1080x1920 (세로형)
- **Color Space**: Linear
- **빌드 타겟**: Windows Desktop (기본), WebGL (보조)

## 코딩 룰 (우선순위 순)

### 1. KISS & DRY 원칙 (최우선)
- Keep It Simple, Stupid - 단순하게, 과도한 엔지니어링 금지
- Don't Repeat Yourself - 반복되는 로직은 메서드로 추출
- YAGNI - 당장 필요하지 않은 기능은 구현하지 않음
- 변수와 메서드명은 의도를 명확히 드러내는 이름 사용

### 1-1. 단일 진실의 원천 (Single Source of Truth)
- 데이터나 상태는 하나의 소스에서만 관리
- 동일한 데이터를 여러 곳에서 중복 저장하지 않음
- 상태 동기화 문제 방지를 위해 중앙화된 상태 관리 사용

### 2. Unity Lifecycle Safety
- 생명주기 메서드 중복 금지 (Update, Start, Awake 등)
- OnDestroy에서 새로운 GameObject 생성 금지
- `[SerializeField] private` 필드와 프로퍼티 접근 사용 (public 필드 지양)
- `Awake()` → 자기 자신 초기화, `Start()` → 다른 오브젝트와의 연결

### 3. Null Safety & Error Logging
- 객체/컴포넌트 접근 전 항상 null 체크
- 예상치 못한 상황은 `Debug.LogError`/`LogWarning`으로 로깅
- 프로덕션 코드에서 NullReferenceException 방지
- Unity 오브젝트 null 체크는 `== null` 사용 (`is null` 대신)

### 4. Short Methods & Single Responsibility
- 메서드는 30줄 이하로 유지
- 하나의 메서드는 하나의 명확한 책임만
- 복잡한 로직은 작은 헬퍼 메서드로 분리
- SOLID 원칙 준수

### 5. Component Caching
- Update나 FixedUpdate에서 `GetComponent`/`Find` 사용 금지
- `Awake()`에서 컴포넌트 참조 캐싱
- 가능하면 `TryGetComponent` 사용
- 코루틴의 `WaitForSeconds` 등은 캐싱하여 재사용

### 6. Basic Error Prevention
- 누락된 using 문 체크
- 메서드 호출 전 메서드명 존재 여부 검증
- 컴파일 에러 없음을 항상 확인
- string 비교 대신 `Animator.StringToHash()` 사용

### 7. Side Effect Awareness
- 변경 시 기존 코드에 미치는 영향 고려
- 다른 스크립트와의 상호작용 검토
- 제안 시 잠재적 위험 언급

## 네이밍 컨벤션

- **클래스/구조체**: PascalCase (`DialogueManager`, `CharacterData`)
- **public 메서드/프로퍼티**: PascalCase (`ShowDialogue()`, `CurrentCharacter`)
- **private 필드**: _camelCase (`_dialogueIndex`, `_currentAffection`)
- **로컬 변수/매개변수**: camelCase (`characterName`, `selectedChoice`)
- **상수**: PascalCase (`MaxAffection`, `DefaultDialogueSpeed`)
- **enum**: PascalCase, 멤버도 PascalCase (`CharacterType.MainHeroine`)
- **인터페이스**: I 접두사 (`IInteractable`, `ISaveable`)
- **파일명**: 클래스명과 동일 (1파일 1클래스 원칙)

## 코드 스타일

- 중괄호는 Allman style (새 줄에서 시작)
- `var` 사용은 타입이 명확한 경우에만 허용
- 주석은 한국어로 작성
- 에디터 스크립트에서 씬 수정 시 `Undo.RecordObject()` 사용

## 아키텍처 가이드라인

### Design Patterns
- **Singleton**: GameManager, AudioManager 등 전역 매니저에만 제한적 사용
- **Observer Pattern**: 이벤트 기반 통신 (C# event, UnityEvent)
- **ScriptableObject**: 게임 데이터 정의 (캐릭터 정보, 대화 데이터 등)
- **State Machine**: 게임 상태, 캐릭터 상태 관리

### 연애 시뮬레이션 핵심 시스템
- **대화 시스템**: ScriptableObject 기반 대화 데이터
- **호감도 시스템**: 캐릭터별 호감도 수치 관리
- **선택지 시스템**: 분기 선택에 따른 결과 처리
- **이벤트 시스템**: 조건 기반 이벤트 트리거
- **세이브/로드**: JSON 직렬화 기반 저장 시스템

## 에러 처리
- 저장 실패 시 사용자에게 적절한 피드백 제공
- 모든 외부 리소스 로드에 try-catch 또는 null 체크 적용
- 실패 빠르게 감지하고 명확한 에러 메시지 제공

## MCP 활용
- MCP Unity 도구를 적극 활용하여 씬 편집, 게임오브젝트 조작, 테스트 실행
- 씬 수정 후 반드시 `save_scene` 호출
- 스크립트 수정 후 `recompile_scripts`로 컴파일 확인

## UI 규칙
- Canvas Scaler: Scale With Screen Size (1080x1920 기준, 세로형)
- 한국어 텍스트: TextMeshPro + 한글 폰트 에셋 사용
- 2D 프로젝트이므로 Z축은 레이어 정렬 용도로만 사용

## 챕터(대화 데이터) 작성 규칙

### 1. 새 챕터 작성 전 필수 확인
- `Assets/Resources/Dialogues/chapter_summary.md`를 먼저 읽고 전체 스토리 흐름을 파악
- 직전 2~3개 챕터 JSON을 반드시 읽고, 구조·톤·설정을 파악한 뒤 작성
- 등장인물 이름, 말투, 호칭, 관계 설정이 기존과 일치하는지 확인
- 진행 중인 플래그(`flagToSet`) 흐름이 끊기지 않도록 이전 챕터의 플래그 확인
- **새 챕터 작성 완료 후 `chapter_summary.md`에 해당 챕터 요약을 반드시 추가**

### 2. JSON 구조 규칙
- 루트 필드: `dialogueId`, `chapterTitle`, `sections` (필수)
- `dialogueId`: `"chapter{번호}"` (예: `"chapter16"`)
- `chapterTitle`: `"Chapter{번호}\n{한글 부제}"` 형식
- 반드시 `sections` 기반 구조 사용 (flat `lines` 방식 사용 금지)
- 첫 번째 섹션 키는 항상 `"start"`

### 3. DialogueLine 필드 규칙
- 필수: `speaker` (캐릭터명 또는 나레이션은 `""`), `text`
- `emotion` 허용값: `"neutral"`, `"smile"`, `"angry"`, `"sad"`, `"curious"`, `"surprised"`, `"embarrassed"`
- `characters[].position` 허용값: `"Left"`, `"Center"`, `"Right"` (PascalCase)
- `characters[].id` 허용값: `"adelin"`, `"duke"`, `"count"`, `"kaidel"` 등 기존 ID 사용
- `textAlign` 허용값: `"left"`, `"center"`, `"right"` (소문자)
- `pause: true` 사용 시 `text`는 빈 문자열 `""`로

### 4. Speaker 이름 규칙 (ConvertSpeakerToId 매핑 기준)
- 주인공: `"me"`
- 아델린: `"아델린"` 또는 `"낯선 여인"` → ID `"adelin"`
- 대공: `"대공"` 또는 `"공작"` → ID `"duke"`
- 백작: `"백작"` → ID `"count"`
- 에르하: `"에르하"` → ID `"kaidel"`
- 렌: `"렌"`, 루카스: `"루카스"`, 클라라: `"클라라"`
- 새 캐릭터 추가 시 `DialogueManager.ConvertSpeakerToId()`에 매핑 추가 필수

### 5. 선택지(Choice) 규칙
- **한 챕터당 최소 4회 이상의 선택지 이벤트 필수** (플레이어 참여감 유지)
- 각 선택지 이벤트의 선택지 개수는 2~3개로 구성
- 선택지 분기 후 답변(섹션)은 짧게 유지 — 5~8줄 이내, 호흡이 길어지지 않도록
- `goto`: 같은 챕터 내 섹션 이동 (섹션 키가 실제 존재해야 함)
- `nextDialogueId`: 다음 챕터로 이동 시 사용
- `affectionChange` 범위: -2 ~ +15 (기존 패턴 유지)
- `currencyCost`: 유료 선택지 (보통 5), 0이면 무료
- `flagToSet` 형식: `"ch{챕터번호}_{설명}"` (예: `"ch16_trust_lucas"`)
- 선택지가 1개면 자동 실행됨 → 분기 없이 "계속하기" 용도로 활용

### 6. 배경(Background) 규칙
- 기존 배경 ID 우선 재사용: `"castle_hall"`, `"count_manor_hall"`, `"count_study"`, `"emilia_room"`, `"lakeside"`, `"forest_road"`, `"market_street"`, `"guild_hall"`, `"guild_private_room"`, `"dragon_nest"`, `"dragon_consciousness"`, `"underwater"`, `"palace_exterior"`, `"carriage_interior"`, `"carriage_exterior"`, `"dark_alley"`, `"basement"`, `"underground_prison"`, `"prison_cell"`, `"slave_market"`, `"execution_ground"`, `"mirror_room"`, `"unfamiliar_bedroom"`, `"castle_dishroom"`
- 새 배경 추가 시 `Resources/Backgrounds/{id}.png` 파일 필요
- 배경 변경은 해당 라인의 `"background"` 필드로 지정 (이후 라인에서 자동 유지)

### 7. 설정 일관성 체크리스트 (새 챕터 작성 완료 후)
- [ ] 직전 챕터의 마지막 상황과 자연스럽게 이어지는가?
- [ ] 캐릭터 이름·호칭이 기존과 동일한가?
- [ ] 사용한 emotion, background, character ID가 모두 유효한 값인가?
- [ ] `goto`가 가리키는 섹션이 실제 존재하는가?
- [ ] `flagToSet` 네이밍이 `ch{N}_{desc}` 형식을 따르는가?
- [ ] 챕터 마지막에 다음 챕터로의 연결(`nextDialogueId`) 또는 종료 처리가 있는가?

## 캐릭터 컨셉

### 세계관 핵심 설정
- 주인공은 전생에서 **대공비**(공작의 아내)였던 본인. 죽은 뒤 1년 전 과거로 회귀
- 회귀 후 **에밀리아의 몸**에 들어감. 에밀리아 = 아델린(가명). 동일 인물
- 아델린(에밀리아)은 원래 공작을 유혹하여 대공비를 외롭게 만든 장본인
- 주인공은 소설 지식이 아닌, **직접 겪은 미래**를 알고 있음
- 목표: 전생의 비극을 반복하지 않고, 자신의 운명을 바꾸는 것

### 공략 대상
- **공작(대공)**: 주인공(대공비)의 전생 남편. 따뜻함이 있으나 과거엔 아델린(에밀리아)에게 빠져 대공비를 홀대함. 과묵하고 절제된 감정 표현. speaker: `"공작"` 또는 `"대공"`
- **에르하**: 목걸이에 깃든 천년 된 용. 들뜨고 질투 많고 솔직함. 에밀리아에게 점점 빠져드는 중. 다른 남자에게 격하게 반응. speaker: `"에르하"`
- **카이델**: 백작가 소속 마법사. 미스터리한 분위기, 전생/시간축에 대한 지식이 있는 듯. 존댓말 사용, 감정을 숨기지만 에밀리아 앞에서 가끔 흔들림. speaker: `"카이델"`
- **렌**: 정보 길드 길드장. 은발, 사교적이면서 날카로움. 에밀리아에게 호감, 실질적 도움을 주는 타입. speaker: `"렌"`
- **기사단장**: 공작의 충직한 부하. 과묵한 호위 타입. 과거 대공비(주인공 전생)를 몰래 사모했으며, 에밀리아에게서 대공비의 그림자를 느끼고 묘한 끌림을 가짐. speaker: `"기사단장"`

### 주요 인물
- **백작**: 에밀리아의 양부. 냉정하고 계산적. 에밀리아를 도구로 취급. 노예 경매 연루.
- **황비**: 흑막. 어둠의 마나 보유. 공식석상 외에는 모습을 드러내지 않음. 백작의 노예 경매 연루가 드러나자 경고로 도적 습격을 보냄.
- **황녀**: 성년식의 주인공. 화사하고 밝은 성격.
- **아델린 = 에밀리아**: 동일 인물. 아델린은 가명. 원래는 공작을 유혹해 대공비를 외롭게 만든 장본인이었으나, 현재는 주인공의 영혼이 들어가 있음.
