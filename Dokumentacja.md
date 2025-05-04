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

## 6. Szczegóły

### a. Technologia gry
- Gra została stworzona na silniku Unity wersja 6000.0.39f1
- Cały kod gry jest napisany w C#
- Gra używa technologii Unity DOTS (Data Oriented Technology Stack), co pozwala na uzyskanie znacznie lepszej wydajności gry.
- Gra do obsługi elementów gry, oprócz interfejsu, używa systemu ECS (Entity Component System), który jest częścią DOTS.
- Dzięki DOTS jest też możliwe wykożystanie wielu wątków procesora, czego gra używa np. do wyszukiwania ścieżek jednostek.

### b. Grafika i Muzyka
Cała grafika i muzyka w grze (z wyjątkiem czcionek) została stworzona przeze mnie, na potrzeby tej gry.
Czcionki pochodzą z Google Fonts i są dostępne na licencji OFL.

### c. Ogólne działanie gry
- Wszystkie mapy w grze to siatki 128 na 128.
- Podstawowym elementem gry jest "Jednostka", jednostki mogą przemieszczać się i atakować jednostki przeciwnej drużyny.
- Jednostki mają 2 tryby: tryb obrony i tryb ataku. Jeśli tryb obrony jest włączony, jednostka ruszy się jedynie gdy gracz ją przemieści. Jeśli tryb ataku jest włączony, jednostka będzie przemieszczała się w stronę przeciwników, na odległość wystarczającą do ataku. Trzecim trybem jest zatrzymanie jednostki, po zatrzymaniu tryb jednostki jest ustawiany na tryb obrony.
- W grze są 3 typy budynków: Budynki zasobów (Produkują zasoby), Spawnery (Produkują jednostki za zasoby) I Strefy (Zawierają inne budynki, są przejmowane przez jednostki)
- Podstawową jednostką czasu w grze jest "Tick", który dowyślnie trwa 0.5s, jednak gracz może zmienić prędkość czasu na x0.5, x1, x2 lub x4. Podczas 1 Ticku odbywa się m.in. Atakowanie jednostek, właściwe przemieszczanie jednostek, - - produkcja jednostek i przejmowanie stref.
- Inną jednostką czasu jest "Subtick", który wykonuje się 4 razy na tick. Subtick jest odpowiedzialny głównie za animacje i nie jest on wymagany do działania gry, dlatego Subticki są wyłączane przy przędkości czasu x4 dla lepszej wydajności.
- Gracz może zaznaczać jednostki, przemieszczać jednostki, zmieniać tryb jednostek, oraz wybierać typ i ilość produkcji jednostek
- Bot ma takie same możliwości co gracz, więcej o działaniu bota później.

### d. Jednostki
#### Działanie jednostek
Jednostki są produkowane przez spawnery za zasoby. Mają różne koszty i czasy produkcji.
Jednostki podczas 1 ticku mogą poruszyć się o 1 kratkę, także na ukos, jeśli nie jest ona zajęta.
Jednostki wyszukują ścieżki na 2 sposoby:
- A* pathfinding - dla przemieszczenia jednostki przez gracza (lub bota), a także do znajdowania obejścia jeśli wcześniej znaleziona droga jest zajęta przez inne jednostki.
- Breath first search - dla trybu ataku, czyli przemieszczania się w stronę przeciwnika. Jednosti w trybie ataku znajdują przeciwników do odległości 12 kratek.
Jednostki nie mogą atakować w ruchu.
Jednostki mogą atakować raz na tick, niektóre jednostki po ataku lub po ruszeniu muszą odczekać 1 lub więcej tick aż będą mogły zadać kolejny atak.
Jednostki mają 4 typy ataku:
- Bliski atak 1: jednostka atakuje przeciwnika oddalanego o 1 kratkę, ma 20% szansy na trafienie krytyczne, które zadaje 2 razy więcej obrażeń.
- Bliski atak 2: jednostka atakuje przeciwników oddalonych o 1 kratkę, atakuje trzy kratki, które są obok siebie, jednocześnie. Jednostka wybiera 3 kratki na których jest najwięcej jednostek. Ma 20% szansy na trafienie krytyczne.
- Bliski atak 3: jednostka atakuje przeciwnika oddalanego o 2 kratkę, ma 20% szansy na trafienie krytyczne.
- Daleki atak: jednostka atakuje przeciwnika oddalonego o różną ilość kratek, Zasięg różmi się między jesnostkami. jeśli odległość od przeciwnika jest większa niż połowa zasięgu, jest 40% szansy na nietrafienie. Jeśli przeciwnik jest bliżej, szansa na nietrafienie to 20%.
Jednostki mają różną ilość punktów HP. Pozostałe HP jest pokazywane w postaci paska życia pod każdą jednostką.

#### Lista jednostek
W grze jest 12 typów jednostek:
| Nazwa                 | Koszt | Czas produkcji | HP   | Typ Ataku     | Dmg/Tick | Dmg   | Zasięg | Cooldown po ataku | Cooldown po ruszeniu |  
|:---------------------:|:-----:|:--------------:|:----:|:-------------:|:--------:|:-----:|:------:|:-----------------:|:--------------------:|
| Chłop z mieczem       | 50    | 4              | 8    | Bliski atak 1 | 1        | 1     | 1      | 0                 | 0                    |
| Chłop z siekierą      | 60    | 4              | 8    | Bliski atak 2 | 0.5      | 1     | 1      | 1                 | 0                    |
| Chłop z kosą          | 60    | 4              | 6    | Bliski atak 3 | 1        | 1     | 2      | 0                 | 1                    |
| Chłop z łukiem        | 60    | 4              | 4    | Daleki atak   | 0.33     | 1     | 6      | 2                 | 1                    |
| Żołnież z mieczem     | 100   | 6              | 16   | Bliski atak 1 | 2        | 2     | 1      | 0                 | 0                    |
| Żołnież z siekierą    | 120   | 6              | 16   | Bliski atak 2 | 1        | 2     | 1      | 1                 | 0                    |
| Żołnież z włócznią    | 120   | 6              | 12   | Bliski atak 3 | 2        | 2     | 2      | 0                 | 1                    |
| Łucznik               | 120   | 6              | 8    | Daleki atak   | 0.66     | 2     | 8      | 2                 | 1                    |
| Rycerz                | 360   | 12             | 48   | Bliski atak 1 | 4        | 8     | 1      | 1                 | 0                    |
| Viking                | 240   | 10             | 24   | Bliski atak 2 | 2        | 2     | 1      | 0                 | 0                    |
| Strażnik              | 240   | 10             | 32   | Bliski atak 3 | 3        | 6     | 2      | 1                 | 1                    |
| Kusznik               | 300   | 8              | 12   | Daleki atak   | 1.2      | 6     | 10     | 4                 | 0                    |


### e. Budynki
W grze są 3 typy budynków:
#### Budynki zasobów
Budynki zasobów produkują określoną wartość azbestu co tick.
Lista budynków:
- Dom (2 Azbest/Tick)
- Młyn (4 Azbest/Tick)
- Kopalnia (8 Azbest/Tick)

#### Spawnery
Spawnery produkują jednostki za zasoby. Gracz może ustawić ilość i typ produkowanych jednostek.
W grze jest tylko jeden Spawner: Obóz 

#### Strefy
W grze jest tylko jeden typ stref. W strefach mogą znajdować się wyżej wymienione budynki. Strefy są przejmowane przez jednostki. Strefy mają różne ilości wymaganych punktów przejęcia (100-1000). Punkty przejęcia są naliczane co Tick i są równe ilości jednostek przejmującego w strefie odjąć ilość jednostek broniącego, maksymalna wartość to 10 na Tick. Jeśli przejmujący ma mniej jednostek w strefie od broniącego, lub przejmujący ma zero jednostek w strefie, punkty przejęcia są resetowane.
Strefy stanowią jedyny punkt odniesienia na mapie dla Bota, który wysyła jednostki jedynie do stref.

### f. Bot
Bot, lub "AI" (Artificial Idiot) jest dosyć prosty. Bot ma takie same możliwości co gracz, czyli może zaznaczać jednostki, przemieszczać jednostki, zmieniać tryb jednostek, oraz wybierać typ i ilość produkcji jednostek, dodatkowo, bot zdobywa taką samą ilość zasobów z budynków co gracz.

- Bot ma kilka parametrów które różnią się między poziomami.
- Na początku Bot przechodzi przez wszystkie Spawnery które kontroluje. Jeśli któryś nie produkuje aktualnie jednostek (ma kolejkę = 0), Bot losuje typ jednostek oraz ich liczbę (3 - 6), aż trafi na kombinacje na którą ma wystarczająco zasobów. Bot ma też losową szansę na nie wybranie żadnego typu i włączenie trybu oszczędzania, trwającego ilość Ticków zależną od parametrów. Wyprodukowane jednostki zostaną przypisane do "Formacji". Po wybraniu, Bot losuje czy dalej powiększać formację czy wysłać ją do boju.
- Potem, bot przechodzi przez wszystkie Formacje. Jeśli formacja została ukończona i nie ma jeszcze przypisanego celu. Bot wybiera cel z listy stref. Wybierany jest najbliższy cel, jednak jest szansa (zależna od parametru) na odrzucenie celu. Szansa na to, czy bot wyśle formację do celu który już kontroluje, też jest paramertem. Bot ustawia jednostkom tryb ataku jeśli wysyła je do strefy której nie kontroluje, w przeciwnym wypadku ma 50% szansy na ustawienie jednostek w tryb obrony.
- Jeśli formacja przejęła strefę, do której została wysłana, bot może odrazu wysłać jednostki do kolejnej strefy, lub pozostawić je w przejętej strefie. Szansa na to też jest parametrem.
- Jest też mała szansa, że bot "poprawi" pozycję jednostek, czyli jeszcze raz przemieści je do strefy, oraz jeszcze mniejsza szansa na to, że bot zmieni cel formacji.
- Na koniec, bot wybiera jednostki z formacji dla której został ustawiony nowy cel i je przemieszcza.
- Jeśli jednostka bota została zaatakowana, jej tryb jest ustawiany na tryb ataku.


