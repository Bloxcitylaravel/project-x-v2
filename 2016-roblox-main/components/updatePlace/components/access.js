import updatePlaceStore from "../stores/updatePlaceStore";
import { useEffect, useState } from "react";
import ActionButton from "../../actionButton";
import useButtonStyles from "../../../styles/buttonStyles";
import { setUniverseMaxPlayers, setUniverseYear } from "../../../services/develop";

const Access = (props) => {
  const s = useButtonStyles();
  const store = updatePlaceStore.useContainer();
  const [maxPlayers, setMaxPlayers] = useState(10);
  const [feedback, setFeedback] = useState(null);
  const [year, setSelectedYear] = useState(2016);
  
  const resetForm = () => {
    setFeedback(null);
    setMaxPlayers(store.details.maxPlayerCount);
    setSelectedYear(store.details.year);
  };

  const save = () => {
    store.setLocked(true);
    setFeedback(null);
    setUniverseYear({
      universeId: store.details.universeId,
      year: year,
    });
    setUniverseMaxPlayers({
      universeId: store.details.universeId,
      maxPlayers: maxPlayers,
    })
      .then(() => {
        window.location.reload();
      })
      .catch((e) => {
        store.setLocked(false);
        setFeedback(e.message);
      });
  };

  useEffect(() => {
    resetForm();
  }, [store.details]);

  return (
    <div className="row mt-4">
      <div className="col-12">
        <h2 className="fw-200f mb-4">Access</h2>
        {feedback ? <p className="text-danger">{feedback}</p> : null}
        <div>
          <p className="fw-bold">Maximum Visitor Count:</p>
          <select
            value={maxPlayers}
            className="br-none border-1 border-secondary pe-2"
            onChange={(v) => {
              setMaxPlayers(parseInt(v.currentTarget.value, 10));
            }}
          >
            {[...new Array(96)].map((_, i) => {
              return (
                <option value={i + 5} key={i}>
                  {i + 5}
                </option>
              );
            })}
          </select>
          <p className="fw-bold">Select Year:</p>
          <select
            value={store.details.year}
            className="br-none border-1 border-secondary pe-2"
            onChange={(v) => {
              setSelectedYear(parseInt(v.currentTarget.value));
            }}
          >
            <option value={2015}>2015</option>
            <option value={2016}>2016</option>
            <option value={2017}>2017</option>
            <option value={2018}>2018</option>
            <option value={2020}>2020</option>
            <option value={2021}>2021</option>
          </select>
        </div>
        <div className="mt-4">
          <div className="d-inline-block">
            <ActionButton
              disabled={store.locked}
              className={s.normal + " " + s.continueButton}
              label="Save"
              onClick={save}
            />
          </div>
          <div className="d-inline-block ms-4">
            <ActionButton
              disabled={store.locked}
              className={s.normal + " " + s.cancelButton}
              label="Cancel"
              onClick={() => {
                resetForm();
              }}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default Access;
