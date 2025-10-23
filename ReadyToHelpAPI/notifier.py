from flask import Flask, request, jsonify
from datetime import datetime
app = Flask(__name__)

@app.post("/notify")
def notify():
    r = request.get_json(silent=True) or {}
    data = { (k.lower() if isinstance(k,str) else k): v for k,v in r.items() }

    target = data.get("entityname") or str(data.get("type") or "-")
    ent_id = data.get("entityid") or "-"
    occ_id = data.get("occurrenceid")
    title  = data.get("title","")
    lat    = data.get("latitude")
    lon    = data.get("longitude")
    msg    = data.get("message","")

    print(f'[{datetime.utcnow().isoformat()}] -> {target} (EntId:{ent_id}) | Occ:{occ_id} | {title} @ ({lat},{lon}) :: {msg}')
    return jsonify(ok=True)

app.run(host="localhost", port=5088)