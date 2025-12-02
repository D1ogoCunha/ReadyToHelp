describe('Occurrence Detail Page', () => {
  // Mock occurrence data to simulate API response
  const mockOccurrence = {
    id: 123,
    title: 'Flooding in Downtown',
    description: 'Heavy rain caused flooding in the main square.',
    type: 'FLOOD',
    status: 'ACTIVE',
    priority: 'HIGH',
    latitude: 40.1234,
    longitude: -8.5678,
    creationDateTime: '2023-10-27T10:00:00Z',
    endDateTime: null,
    reportCount: 5,
    responsibleEntityId: 42,
  };

  beforeEach(() => {
    cy.intercept('GET', '**/api/occurrence/123', {
      statusCode: 200,
      body: mockOccurrence,
    }).as('getOccurrenceDetails');
  });

  it('should display occurrence details correctly', () => {
    // Visit the page
    cy.visit('/occurrence/123');
    cy.wait('@getOccurrenceDetails');

    // Verify Title (Updated selector: .od-title)
    cy.get('h1.od-title').should('contain.text', 'Flooding in Downtown');

    // Verify Description (Updated selector: .description-text)
    cy.get('.description-text').should(
      'contain.text',
      'Heavy rain caused flooding'
    );

    // Verify General Info Table
    // We look for the label (info-label) and verify the next sibling (info-value)

    // Type
    cy.contains('.info-label', 'Type')
      .next('.info-value')
      .should('contain.text', 'Flood');

    // Status (Active should be green/success)
    cy.contains('.info-label', 'Status')
      .next('.info-value')
      .find('.text-success')
      .should('contain.text', 'Active');

    // Priority (High should be red/danger)
    cy.contains('.info-label', 'Priority')
      .next('.info-value')
      .find('.text-danger')
      .should('contain.text', 'High');

    // Coordinates
    cy.contains('.info-label', 'Coordinates')
      .next('.info-value')
      .should('contain.text', '40.1234, -8.5678');

    // Responsible Entity ID
    cy.contains('.info-label', 'Responsible Entity')
      .next('.info-value')
      .should('contain.text', '42');
  });

  it('should display the static map image', () => {
    cy.visit('/occurrence/123');
    cy.wait('@getOccurrenceDetails');

    // Verify image (Updated selector: .map-preview-box img)
    cy.get('.map-preview-box img')
      .should('be.visible')
      .and('have.attr', 'src')
      .and('include', 'api.mapbox.com');
  });

  it('should handle error state gracefully', () => {
    // Simulate 404
    cy.intercept('GET', '**/api/occurrence/999', {
      statusCode: 404,
    }).as('getOccurrenceError');

    cy.visit('/occurrence/999');
    cy.wait('@getOccurrenceError');

    // Verify Error State
    cy.get('.state-container').should('be.visible');

    // Verify 'Error' title
    cy.get('.state-container h3.text-danger').should('contain.text', 'Error');

    // Verify error message
    cy.get('.state-container p').should(
      'contain.text',
      'Unable to load the details of this occurrence.'
    );
  });

});
