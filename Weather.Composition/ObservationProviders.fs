module Weather.CompositionRoot.ObservationProviders

open Weather.DataProvider
open Weather.HttpClient

module Ogimet = 
    let fetchObservationsByInterval = 
        OgimetObservationsProvider.fetchObservationsByInterval 
            HttpClient.httpGet
