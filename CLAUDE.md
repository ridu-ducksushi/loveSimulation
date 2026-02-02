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
