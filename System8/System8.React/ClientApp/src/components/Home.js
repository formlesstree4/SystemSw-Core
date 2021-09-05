import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = {mappings: [], loading: false};

    this.onItemClick.bind(this);
    this.getClassName.bind(this);
    this.fetchMappings.bind(this);
  }

  componentDidMount() {
    this.fetchMappings();
  }


  render () {
    return this.state.loading
      ? <p><em>Loading...</em></p>
      : this.renderMappingList(); 
  }


  renderMappingList() {
    return (
      <div className="container">
        <ul>
          {this.state.mappings.map(mapping =>
            <li className={this.getClassName} onClick={this.onItemClick}>{mapping.channelName}</li>  
          )}
        </ul>
        <ul>
          <li onClick={this.fetchMappings}>Refresh</li>
        </ul>
      </div>
    )
  }

  getClassName(mapping) {
    if (mapping.isSelected) {
      return 'selected';
    }
    return '';
  }

  async fetchMappings() {
    this.setState({loading: true});
    const response = await fetch('switcher');
    const data = await response.json();
    this.setState({ mappings: data, loading: false });
  }

  async onItemClick(entry) {
    this.setState({loading: true});
    const response = await fetch('switcher', {
      method: 'POST',
      body: JSON.stringify(entry)
    });
    const data = await response.json();
    this.setState({ mappings: data, loading: false });
  }


}
