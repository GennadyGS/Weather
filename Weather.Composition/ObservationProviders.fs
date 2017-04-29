module Weather.CompositionRoot.ObservationProviders

open Weather.DataProvider
open Weather.HttpClient

let fetchObservationsByIntervalFromOgimet = 
    OgimetObservationsProvider.fetchObservationsByInterval 
        HttpClient.httpGet
