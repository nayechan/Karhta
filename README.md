# 개요

**Kahrta**는 무한한 세계에서 모험을 시작할 수 있는 3D 오픈월드 로그라이크 게임입니다. 빠르고 반복적인 플레이를 특징으로 하며, 절차적 지형 생성을 기반으로 다양한 게임플레이를 제공합니다. 개발 과정에서 **Job System**, **Object Pooling** 등 최신 기술을 활용했으며, 직관적이고 효율적인 아이템 관리 시스템과 객체 설계를 목표로 삼았습니다.

절차적 지형 생성 기반 3D 로그라이크 RPG

- 사용 엔진 / 프레임워크 : Unity
- 기간 : 2024. 5. 30 ~ 2024. 8. 30

---

# 주요 시스템

## 1. **지형 생성 시스템**

### a) Heightmap 생성

- **Simplex Noise**를 활용하여 빠르고 자연스러운 랜덤 높이 값을 생성.
- **노이즈 가공**:
    - **Offset 설정**: 현실적인 지형 생성을 위해 각 계층 노이즈에 Bitmask를 적용하여 노이즈 값을 조정.
    - **주파수/진폭 조정**: Frequency와 Amplitude 값을 조정한 후, 각 레이어를 합산해 균형 잡힌 지형을 생성.

### b) Terrain 생성

- **Job System**과 **UniTask**를 활용하여 비동기 처리 및 멀티스레딩으로 지형 생성.
- **우선순위 큐**를 통해 생성된 Heightmap 데이터를 관리하며, 업데이트 시 최우선 데이터부터 GameObject로 변환.

### c) Terrain Pooling

- 각 **Chunk**와 물 객체는 **ChunkPoolManager**와 **WaterPoolManager**에서 관리하여 메모리 효율성을 극대화.

---

## 2. **전투 시스템**

### a) 에셋 관리

- **ScriptableObject**와 **Addressable Assets**를 활용하여 아이템 데이터를 표준화.
- **AddressableHelper**를 통해 에셋을 주소와 매핑하여 관리 및 초기화 효율성을 강화.

### b) 전투 진행

- **인터페이스 설계**:
    - `IAttackable`: 공격 가능한 대상 정의.
    - `IDamageable`: 피해를 받을 수 있는 대상 정의.
- **애니메이션 처리**:
    - **Animator**를 통해 무기별 공격 모션을 자동으로 전환.
    - 각 무기마다 고유한 AnimationClip 설정.
- **적 AI 경로 탐색**:
    - 지형이 랜덤으로 생성되기 때문에 **NavMesh** 대신 A* 알고리즘을 사용하여 Mob이 Player를 추적.

---

## 3. 추후 발전 및 활용 방안

### a) Procedural Generation 강화

- Heightmap이 아닌 복셀 기반의 3D 메시를 직접 생성하여 관리하는 방식으로, 좀더 현실적인 지형을 생성
    - Marching cube/Dual Contouring과 같은 알고리즘을 활용하여 복셀 데이터를 효율적으로 관리
    - Octree를 활용한 LOD를 통해 퍼포먼스 증가
- Wavefunction collapse를 활용하여 현실적인 구조물 생성

### b) 실제 서비스 가능한 게임으로의 발전

- 세이브/로드 및 네트워크 기능 구현을 통한 완성도 증가
- 각종 텍스쳐 및 캐릭터 / 아이템 어셋 추가
