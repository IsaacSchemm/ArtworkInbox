namespace ArtworkInbox.FurAffinity

open System
open System.IO
open System.Net
open FSharp.Control.Tasks.V2
open Newtonsoft.Json

module Notifications =
    type CurrentUser = {
        name: string
        profile: string
        profile_name: string
    }
    
    type NewSubmission = {
        id: int
        title: string
        thumbnail: string
        link: string
        name: string
        profile: string
        profile_name: string
    }

    type Submissions = {
        current_user: CurrentUser
        new_submissions: NewSubmission list
    }

    let GetSubmissionsAsync fa_cookie sfw from = task {
        let req =
            sprintf "https://faexport.spangle.org.uk/notifications/submissions.json?%s&from=%d" (if sfw then "sfw=1" else "") from
            |> WebRequest.CreateHttp
        req.UserAgent <- "ArtworkInbox/0.0 (https://github.com/IsaacSchemm/ArtworkInbox)"
        req.Headers.Set("FA_COOKIE", fa_cookie)
        use! resp = req.GetResponseAsync()
        use stream = resp.GetResponseStream()
        use sr = new StreamReader(stream)
        let! json = sr.ReadToEndAsync()
        return JsonConvert.DeserializeObject<Submissions> json
    }

    type NotificationCounts = {
        submissions: int
        comments: int
        journals: int
        favorites: int
        watchers: int
        notes: int
        trouble_tickets: int
    } with
        member this.Sum =
            seq {
                this.submissions
                this.comments
                this.journals
                this.favorites
                this.watchers
                this.notes
                this.trouble_tickets
            }
            |> Seq.sum

    type NewJournal = {
        journal_id: int
        title: string
        name: string
        profile: string
        profile_name: string
        posted: string
        posted_at: DateTimeOffset
    }

    type Others = {
        current_user: CurrentUser
        notification_counts: NotificationCounts
        new_journals: NewJournal list
    }

    let GetOthersAsync fa_cookie = task {
        let req = WebRequest.CreateHttp "https://faexport.spangle.org.uk/notifications/others.json"
        req.UserAgent <- "ArtworkInbox/0.0 (https://github.com/IsaacSchemm/ArtworkInbox)"
        req.Headers.Set("FA_COOKIE", fa_cookie)
        use! resp = req.GetResponseAsync()
        use stream = resp.GetResponseStream()
        use sr = new StreamReader(stream)
        let! json = sr.ReadToEndAsync()
        return JsonConvert.DeserializeObject<Others> json
    }