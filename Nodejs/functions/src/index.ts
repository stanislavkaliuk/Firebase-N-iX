import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';
admin.initializeApp();

 export const RequestMessage = functions.https.onCall((token)=>
 {
    const payload = 
    {
        notification:
        {
            title: "Hello N-iX",
            body: "Test message"
        }
    };
    var strToken : string = String(token);
    setTimeout(() => {
        admin.messaging().sendToDevice(strToken, payload).then((response)=>
        {
            alert(response.successCount);
            return true;
        })
        .catch((error)=>{
            return false;
        }); 
    }, 5000);
    
 });
