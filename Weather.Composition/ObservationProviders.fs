module Weather.CompositionRoot.ObservationProviders

open Weather
open Weather.DataProvider
open Weather.HttpClient

module Ogimet = 
    let fetchObservationsByInterval = 
        OgimetObservationsProvider.fetchObservationsByInterval 
            Synop.Parser.parseSynop
            HttpClient.httpGet
