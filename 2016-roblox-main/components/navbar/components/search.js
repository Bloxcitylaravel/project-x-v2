import { useEffect, useRef, useState } from "react";
import { createUseStyles } from "react-jss";

const useSearchIconStyles = createUseStyles({
  icon: {
    float: 'right',
    margin: 0,
    padding: 0,
    border: 'none',
    cursor: 'pointer',
    maxHeight: '28px',
    maxWidth: '28px'
  }
})

const SearchIcon = (props) => {
  const s = useSearchIconStyles();
  return <span className={`col-2 ${s.icon} icon-nav-search`}></span>
}

const useSuggestionEntryStyles = createUseStyles({
  text: {
    paddingLeft: '15px',
    fontSize: '16px',
    paddingTop: '0.5rem',
    paddingBottom: '0.5rem',
    marginBottom: 0,
    fontWeight: 400,
    color: 'rgb(52, 52, 52)',
    '&:hover': {
      boxShadow: '4px 0 0 0 #00a2ff inset',
    },
  },
  link: {
    color: 'rgb(52, 52, 52)',
    textDecoration: 'none',
  }
});

const SearchSuggestionEntry = props => {
  const s = useSuggestionEntryStyles();
  return <a className={s.link + ' '} href={`${props.url}?keyword=${encodeURIComponent(props.query)}`}>
    <p className={s.text}>
      Search "{props.query}" in {props.mode}
    </p>
  </a>
}

const useSuggestionStyles = createUseStyles({
  container: {
    background: 'white',
    position: 'fixed',
    boxShadow: '0 -5px 20px rgb(25 25 25 / 15%)',
    border: '1px solid rgba(0,0,0,0.15)',
    borderRadius: '3px',
    width: 'inherit',
    marginTop: '1px',
  }
});

const SearchSuggestionContainer = props => {
  const input = props.inputRef.current;
  useEffect(() => {

  }, [input.clientWidth]);
  const s = useSuggestionStyles();
  return <div className={s.container} style={{ width: input.clientWidth + 'px' }}>
    <SearchSuggestionEntry mode='Catalog' url='/catalog' query={props.query}></SearchSuggestionEntry>
    <SearchSuggestionEntry mode='People' url='/search/users' query={props.query}></SearchSuggestionEntry>
    <SearchSuggestionEntry mode='Games' url='/games' query={props.query}></SearchSuggestionEntry>
    <SearchSuggestionEntry mode='Groups' url='/My/Groups.aspx' query={props.query}></SearchSuggestionEntry>
    <SearchSuggestionEntry mode='Library' url='/' query={props.query}></SearchSuggestionEntry>
  </div>
}

const useSearchStyles = createUseStyles({
  wrapper: {
    padding: 0,
    background: 'white',
    borderRadius: '3px',
    border: '1px solid rgba(0,0,0,0.15)',
    width: '100%',
    height: '28px!important'
  },
  searchInput: {
    border: 'none',
    padding: 0,
    fontSize: '16px!important',
    marginLeft: '12px',
    marginTop: '2px',
    float: 'left',
    background: 'none',
    color: '#191919',
    fontWeight: '200',
    display: 'block',
    lineHeight: '1.5',
    appearance: 'none',
    borderRadius: '5.25px',
    '&:focus': {
      border: 'none!important',
      boxShadow: 'none!important',
      outline: 0,
    },
    '&::placeholder': {
      color: '#B8B8B8'
    }
  },
  seniorClass: {
    padding: 0,
    margin: 0,
    marginTop: 'auto',
    marginBottom: 'auto',
  }
});

const Search = props => {
  const [query, setQuery] = useState('');
  const [showSearchThing, setShowSearchThing] = useState(false);
  const inputRef = useRef(null);
  const s = useSearchStyles();

  useEffect(() => {
    setShowSearchThing(query !== '');
  }, [query]);

  return <div className={`${s.seniorClass} col-12 col-lg-3`}>
    <div style={{ width: '100%' }}>
      <div className={s.wrapper}>
        {/*<input ref={inputRef} value={query} className={`form-control col-10 ${s.searchInput}`} placeholder='Search' onInput={(v) => {
          setQuery(v.currentTarget.value);
        }}></input>*/}
        <input ref={inputRef} value={query} className={`col-10 ${s.searchInput}`} placeholder='Search' onInput={(v) => {
          setQuery(v.currentTarget.value);
        }}></input>
        <SearchIcon></SearchIcon>
      </div>
      {showSearchThing && <SearchSuggestionContainer query={query} inputRef={inputRef}></SearchSuggestionContainer>}
    </div>
  </div>
}

export default Search;