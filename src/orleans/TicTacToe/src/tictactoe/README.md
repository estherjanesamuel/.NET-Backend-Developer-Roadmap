This sample demonstrates a Web-based Tic tac toe game.

The game is implemented as a single project, tictactoe.csproj, which uses the [].NET Generic Host]() to host an ASP.NET Core MVC application alongside Orleans.

The client side of game is a JavaScript application which polls the MVC application for updates.

```
ajax: {
        createGame: function (cb) {
            $.post("/Game/CreateGame", cb);
        },

        placeMove: function (coords, cb) {
            $.post("/Game/", function (data) {
                if (cb) {
                    cb(data);
                }
            });
        },
        getGames: function (cb) {
            $.get("/Game/Index/" + oxo.rand(), function (data) {
                if (data) {
                    // games playing
                    data[0].forEach(function (x) {
                        x.waiting = x.state == 0;
                    });
                }
                cb({ currentGames: data[0], availableGames: data[1] });
            });
        },
        getMoves: function (cb) {
            $.get("/Game/GetMoves/" + oxo.model.currentGame + oxo.rand(), cb);
        },
        makeMove: function (x, y, cb) {
            $.post("/Game/MakeMove/" + oxo.model.currentGame + "/?x=" + x + "&y=" + y, cb);
        },
        joinGame: function (gameId, cb) {
            $.post("/Game/Join/" + gameId, function (data) {
                // check we have joined
                oxo.model.currentGame = gameId;
                cb(data);
            });
        },
        setName: function (name, cb) {
            $.post("/Game/SetUser/" + name, function (data) {
                if (cb) {
                    cb(data);
                }
            });
        }
    },
```
