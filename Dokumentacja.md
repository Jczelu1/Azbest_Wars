# Azbest Wars

## 1. Opis Gry
Azbest Wars to gra strategiczna czasu rzeczywistego (RTS), stworzona na konkurs "Invent your game 2025".
Jest to gra PvE, w krórej gracz kontroluje armię jednostek i walczy przeciwko armii bota.
Celem gry jest przejście wszystkich poziomów. Powodzenia.

## 2. Wymagania Systemowe
Ta wersja gry jest przeznaczona dla systemu Windows x64

## 3. Instrukcja Kompilacji
- Otwórz (lub zainstaluj) Unity Hub
- W prawym górnym rogu kliknij "Add" i wybierz folder "Azbest Wars Project"
- Zainstaluj Unity 6000.0.39f1 jeśli nie jest zainstalowane
- Otwórz "Azbest Wars Project"
- Po uruchomieniu edytora projektu kliknij File->Build and Run
- Wybierz folder w którym będzie znajdowała się skompilowana gra
- Gra powinna uruchomić się automatycznie

## 4. Instrukcja Uruchomienia
- W folderze "Azbest Wars" znajduje się skompilowana wersja gry
- Aby uruchomić grę należy otworzyć plik "Azbest Wars.exe"

## 5. Jak grać
- Polecam przejść tutorial w grze
- Przypisanie przycisków można zobaczyć klikając w "Sterowanie" w grze

## 6. Poradnik rozszerzony

## 7. Szczegóły

### a. Technologia gry
- Gra została stworzona na silniku Unity wersja 6000.0.39f1
- Cały kod gry jest napisany w C#
- Gra używa technologii Unity DOTS (Data Oriented Technology Stack), co pozwala na uzyskanie znacznie lepszej wydajności gry.
- Gra do obsługi elementów gry, oprócz interfejsu, używa systemu ECS (Entity Component System), który jest częścią DOTS.
- Dzięki DOTS jest też możliwe wykożystanie wielu wątków procesora, czego gra używa np. do wyszukiwania ścieżek jednostek.

### b. Ogólne działanie gry
- Wszystkie mapy w grze to siatki 128 na 128.
- Podstawowym elementem gry jest "Jednostka", jednostki mogą przemieszczać się i atakować jednostki przeciwnej drużyny.
- Jednostki mają 2 tryby: tryb obrony i tryb ataku. Jeśli tryb obrony jest włączony, jednostka ruszy się jedynie gdy gracz ją przemieści. Jeśli tryb ataku jest włączony, jednostka będzie przemieszczała się w stronę przeciwników, na odległość wystarczającą do ataku. Trzecim trybem jest zatrzymanie jednostki, po zatrzymaniu tryb jednostki jest ustawiany na tryb obrony.
- W grze są 3 typy budynków: Budynki zasobów (Produkują zasoby), Spawnery (Produkują jednostki za zasoby) I Strefy (Zawierają inne budynki, są przejmowane przez jednostki)
- Podstawową jednostką czasu w grze jest "Tick", który dowyślnie trwa 0.5s, jednak gracz może zmienić prędkość czasu na x0.5, x1, x2 lub x4. Podczas 1 Ticku odbywa się m.in. Atakowanie jednostek, właściwe przemieszczanie jednostek, - - produkcja jednostek i przejmowanie stref.
- Inną jednostką czasu jest "Subtick", który wykonuje się 4 razy na tick. Subtick jest odpowiedzialny głównie za animacje i nie jest on wymagany do działania gry, dlatego Subticki są wyłączane przy przędkości czasu x4 dla lepszej wydajności.
- Gracz może zaznaczać jednostki, przemieszczać jednostki, zmieniać tryb jednostek, oraz wybierać typ i ilość produkcji jednostek
- Bot ma takie same możliwości co gracz, więcej o działaniu bota później.

### c. Jednostki
#### Działanie jednostek
Jednostki podczas 1 ticku mogą poruszyć się o 1 kratkę, także na ukos, jeśli nie jest ona zajęta.
Jednostki wyszukują ścieżki na 2 sposoby:
- A* pathfinding - dla przemieszczenia jednostki przez gracza (lub bota), a także do znajdowania obejścia jeśli wcześniej znaleziona droga jest zajęta przez inne jednostki.
- Breath first search - dla trybu ataku, czyli przemieszczania się w stronę przeciwnika. Jednosti w trybie ataku znajdują przeciwników do odległości 12 kratek.
Jednostki mogą atakować raz na tick, niektóre jednostki po ataku lub po zatrzymaniu muszą odczekać 1 lub więcej tick aż będą mogły zadać kolejny atak.
Jednostki mają 4 typy ataku:
- Bliski atak 1: jednostka atakuje przeciwnika oddalanego o 1 kratkę, ma 20% szansy na trafienie krytyczne, które zadaje 2 razy więcej obrażeń.
- Bliski atak 2: jednostka atakuje przeciwników oddalonych o 1 kratkę, atakuje trzy kratki, które są obok siebie, jednocześnie. Jednostka wybiera 3 kratki na których jest najwięcej jednostek. Ma 20% szansy na trafienie krytyczne.
- Bliski atak 3: jednostka atakuje przeciwnika oddalanego o 2 kratkę, ma 20% szansy na trafienie krytyczne.
- Daleki atak: jednostka atakuje przeciwnika oddalonego o różną ilość kratek, Zasięg różmi się między jesnostkami. jeśli odległość od przeciwnika jest większa niż połowa zasięgu, jest 40% szansy na nietrafienie. Jeśli przeciwnik jest bliżej, szansa na nietrafienie to 20%.
Jednostki mają różną ilość punktów HP. Pozostałe HP jest pokazywane w postaci paska życia pod każdą jednostką.

#### Lista jednostek
W grze jest 12 typów jednostek


### d. Budynki


### e. Bot

### f. Poziomy


