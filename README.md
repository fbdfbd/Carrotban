# Carrotban

소코반(블록밀기) 기반의 퍼즐 게임입니다. 
나무/밭 상호작용과 Undo/Redo를 포함한 코어 플레이, 
서버 연동을 통한 유저 데이터·상점·퀘스트·공지 등의 관리가 특징입니다.

## 개발 환경
- 플랫폼 : Mobile(APK), PC

## 개발 환경
- Unity 2022.3.62f1 (URP 14.0.12)
- DOTween
- The Backend SDK (뒤끝)

## 플레이
- 흐름: 로비 → 스테이지 선택 → 그리드 이동/블록 밀기 → 상호작용(나무/밭) → 클리어/보상 → 로비 복귀
- 소코반 퍼즐 규칙: 1칸 단위 이동, 블록은 밀기만 가능, 공간 점유/충돌 체크
- 목표 달성: 나무를 베어 씨앗 획득 → 밭 상호작용 → 씨앗심기 → 물주기 완료 시 스테이지 클리어
- 하트(에너지) 소모 및 서버 시간 기반 재생성 및 서버 시간 기반 상점 재알림

## 핵심 시스템
- Grid 기반 점유/충돌 체크와 타일 맵 구성
- Command 패턴 기반 Undo/Redo (이동/상호작용 명령)
- 데이터드리븐 스테이지 파이프라인  
  `Assets/Maps.json` → `Tools/Maps/MapJsonToSO` → ScriptableObject
- Backend 게임 데이터 추상화 및 트랜잭션 저장
- 로비 시스템(상점/퀘스트/우편/공지/프로필)과 재화 관리
- DOTween 기반 UI/오브젝트 애니메이션, BGM/SFX 매니저

## 콘텐츠
- 스테이지 20개 (챕터 2개)
- 오브젝트/타일: 블록, 나무, 밭, 벽/울타리 등
- 로비 기능: 상점, 퀘스트, 공지, 프로필, 옵션

## 스크린샷
<img width="394" height="818" alt="캡처_2026_01_17_00_04_57_778" src="https://github.com/user-attachments/assets/29170fdb-64ad-4870-ba70-3cf90d802bdc" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_52_674" src="https://github.com/user-attachments/assets/e1c63451-8962-41a2-a30b-61d6d4e275d3" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_49_983" src="https://github.com/user-attachments/assets/6e127b13-7091-4a30-996c-3bb1d8b268d9" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_46_571" src="https://github.com/user-attachments/assets/7ad6817c-9d51-4d5b-9a5c-c1f82ea5e97a" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_42_288" src="https://github.com/user-attachments/assets/4bd4fc37-da44-4ca1-9b50-af47837048cf" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_37_105" src="https://github.com/user-attachments/assets/d06bbcc3-ce96-4658-aa43-b14209ee485c" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_20_56" src="https://github.com/user-attachments/assets/db9838b8-516d-47e8-9595-87bb1299b3b1" />
<img width="394" height="818" alt="캡처_2026_01_17_00_04_13_212" src="https://github.com/user-attachments/assets/4c1e0842-f54f-49b5-af1b-b40676ab1294" />

