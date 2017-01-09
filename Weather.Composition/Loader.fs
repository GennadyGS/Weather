module Weather.Loader

open FSharp.Data

Http.RequestString("http://www.ogimet.com/cgi-bin/getsynop?block=33345&begin=201701010000&end=201701312359")
