module Weather.Composition.ObservationProviders

open Weather
open Weather.DataProvider
open Weather.HttpClient

module Ogimet = 
    let fetchObservationsByInterval = 
        OgimetObservationsProvider.fetchObservationsByInterval 
            Synop.Parser.safeParseSynop
            HttpClient.httpGet
