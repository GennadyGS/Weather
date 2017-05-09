module Weather.Logic.Database

open Weather
open Weather.Utils
open Weather.Model

let inline readDataContext func connectionString = 
    Utils.Database.readDataContext func connectionString
    >> Result.mapFailure DatabaseError

let inline writeDataContext func connectionString = 
    Utils.Database.writeDataContext func connectionString 
    >> Result.mapFailure DatabaseError

let inline writeDataContextForList func connectionString = 
    Utils.Database.writeDataContextForList func connectionString
    >> Result.mapFailure DatabaseError

let inline unitOfWork func connectionString = 
    Utils.Database.unitOfWork func connectionString 
    >> Tuple.mapSecond (Result.mapFailure DatabaseError)

let inline unitOfWorkForList func connectionString = 
    Utils.Database.unitOfWorkForList func connectionString 
    >> Tuple.mapSecond (Result.mapFailure DatabaseError)
