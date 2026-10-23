[Tools]

Unity 밖에서 쓰는 보조 스크립트. 게임 코드가 아니다.

- deploy-pages.ps1 : WebGL 빌드 폴더(첫 번째 인자, 필수)를 To-steak.github.io 저장소의 docs/public/project05/ 에 복사하고 커밋·push 한다.
                     사이트 저장소가 옆 폴더(../To-steak.github.io)에 없으면 clone 한다. 게임 폴더만 스테이징한다.
                     옵션: -SiteRepoPath, -SiteFolder, -NoPush (커밋만)
- deploy-pages.cmd : deploy-pages.cmd "빌드 폴더" 로 실행하거나, 빌드 폴더를 이 파일 위로 끌어다 놓는다. 실행 정책을 우회해 deploy-pages.ps1 을 실행한다.

배포 주소: https://to-steak.github.io/project05/ (사이트 저장소의 Actions 배포가 끝난 뒤, 보통 1~2분)
