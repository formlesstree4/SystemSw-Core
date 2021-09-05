import React, { Component } from 'react';
import Button from 'react-bootstrap/Button';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);
    this.state = {mappings: [], loading: false};
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
      <div className="d-grid gap-2">
        {this.state.mappings.map(mapping =>
          <Button variant={this.getVariant(mapping)} size="lg" onClick={() => this.onItemClick(mapping)}>{mapping.channelName}</Button>
        )}

        <br />
        <Button variant="primary" size="lg" onClick={() => this.fetchMappings()}>Refresh</Button>
      </div>
    )
  }

  getVariant(mapping) {
    return mapping.isActiveChannel ? "primary" : "secondary";
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
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(entry)
    });
    const data = await response.json();
    this.setState({ mappings: data, loading: false });
  }


}
